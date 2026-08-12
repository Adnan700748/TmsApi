using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using TmsApi.Api.Middleware;
using TmsApi.Api.Options;
using TmsApi.Application.Interfaces;
using TmsApi.Filters;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Behaviors;
using MediatR;
using TmsApi.Api.ExceptionHandlers;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Api.RateLimiting;
using TmsApi.Infrastructure.Transcripts;
using System.Threading.Channels;
using TmsApi.Infrastructure.Workers;
using TmsApi.Application.Transcripts;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;
using TmsApi.Api.Notifications;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddSignalR();


builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//When validating the antiforgery token, look for it in the X-XSRF-TOKEN request heade
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

// Registers a CORS policy that allows the Angular application
// running on localhost:4200 to access this API.

// Load allowed origins from appsettings.Development.json
var allowedOrigins =
    builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>()
    ?? ["http://localhost:4200"];

// Register the CORS policy in the Dependency Injection container
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials() // Vital for HttpOnly auth cookies in Session 2
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// Add HybridCache for stampede protection
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),        // L2 cache TTL (shared cache)
        LocalCacheExpiration = TimeSpan.FromMinutes(2) // L1 cache TTL (in-memory)
    };
});


// Production-only - leave commented in lab
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("Redis");
//     options.InstanceName = "tms:";  // IMPORTANT: prefix to avoid collisions
// });
//
// builder.Services.AddHybridCache();  // This will automatically use Redis

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var (partitionKey, tier) =
                ApiKeyResolver.Resolve(httpContext);

            return tier switch
            {
                ApiKeyTier.Paid =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"paid:{partitionKey}",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 200,
                            TokensPerPeriod = 100,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),

                ApiKeyTier.Free =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"free:{partitionKey}",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 30,
                            TokensPerPeriod = 10,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),

                _ =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"anon:{partitionKey}",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            TokensPerPeriod = 5,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        })
            };
        });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out TimeSpan retry))
        {
            retryAfter = ((int)retry.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType =
            "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "Rate limit exceeded",
                Detail =
                    $"Too many requests. Retry after {retryAfter} seconds.",
                Status = StatusCodes.Status429TooManyRequests,
                Type = "https://tms.local/errors/rate_limit_exceeded"
            },
            ct);
    };
        options.AddConcurrencyLimiter("transcripts", opt =>
    {
        // Maximum concurrent transcript requests
        opt.PermitLimit = 5;

        // Queue up to 20 waiting requests
        opt.QueueLimit = 20;

        // Serve oldest queued request first
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.AddTokenBucketLimiter("search", opt =>
{
    opt.TokenLimit = 10;
    opt.TokensPerPeriod = 5;
    opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
    opt.QueueLimit = 2;
    opt.AutoReplenishment = true;
});

});

// v1/v2 versioning
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description => description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description => description.GroupName == "v2";
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddDbContext<TmsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
                                                              .LogTo(Console.WriteLine, LogLevel.Information)   // Log SQL to output window
                                                              .EnableSensitiveDataLogging());  // Show parameters in query logs (dev only)

// registering the MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));

//registering the FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);

// LoggingBehavior is Registerd  FIRST it must wrap ValidationBehavior
// so LoggingBehavior runs first and wraps validation failures insideits log scope.
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

builder.Services.AddTransient( typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();


builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
builder.Services.AddHostedService<TranscriptWorker>();
builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();

builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Auto-migrate and create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseStatusCodePages();

app.UseExceptionHandler();

app.UseMiddleware<RequestLoggingMiddleware>();


app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

// Enables the CORS policy for incoming requests.
// CRITICAL: Middleware order matters!
// UseRouting-> UseCors-> UseAuthentication-> UseAuthorization
app.UseCors("TmsClient");

app.UseAuthentication();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true ||
        context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery =
            context.RequestServices
                .GetRequiredService<IAntiforgery>();

        var tokens = antiforgery.GetAndStoreTokens(context);

        context.Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken!,
            new CookieOptions
            {
                // Angular JavaScript MUST be able to read this.
                HttpOnly = false,

                Secure = !builder.Environment.IsDevelopment(),

                SameSite = SameSiteMode.Strict
            });
    }

    await next(context);
});

app.UseMiddleware<V1DeprecationMiddleware>();

app.MapControllers();

app.MapHub<TmsHub>("/hubs/tms");


app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))

.RequireAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            // Tell Scalar to pull both documents into its sidebar dropdown
        options
            .AddDocument("v1", "API Version 1.0")
            .AddDocument("v2", "API Version 2.0");
    });
}

app.Run();