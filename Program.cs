using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

 builder.Services.AddControllers(); 

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions,
        TrainingAuthHandler> ("Training", null);

builder.Services.AddAuthorization();

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddOptions<PaymentOptions>()
   .BindConfiguration("Payments")
   .ValidateDataAnnotations()
   .ValidateOnStart();

builder.Services.AddProblemDetails();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

app.UseRouting();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
})
.RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke", async (EnrollmentWorker worker) =>
{
   await worker.ProcessBatch();
   return Results.Ok("processed");
});

// Temporary test endpoints for Exercise 4 (remove after Session 3)
app.MapPost("/api/enrollments/test", async (IEnrollmentService svc, string studentId, string courseCode) =>
{
    var result = await svc.EnrollAsync(studentId, courseCode);
    return Results.Ok(result);
});

app.MapGet("/api/enrollments/test/{id}", async (IEnrollmentService svc, string id) =>
{
    var result = await svc.GetByIdAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});

app.Run();