using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using TmsApi.Enums;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
            .LogTo(Console.WriteLine, LogLevel.Information)   // Log SQL to output window
            .EnableSensitiveDataLogging());                    // Show parameters in query logs (dev only)


builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();

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

app.UseStatusCodePages();

app.UseExceptionHandler();

app.UseMiddleware<RequestLoggingMiddleware>();

// app.UseExceptionHandler("/error");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.MapGet("/api/error", () => 
{ 
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing"); 
});

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))

.RequireAuthorization();

// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith",    GPA = 3.8m, IsActive = true  },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones",      GPA = 2.9m, IsActive = true  },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown",  GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince",   GPA = 3.9m, IsActive = true  },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright",    GPA = 2.5m, IsActive = true  }
        };
        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new() { Code = "CS-101",  Title = "Introduction to Computer Science", Capacity = 30 },
            new() { Code = "CS-201",  Title = "Data Structures and Algorithms",   Capacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I",                       Capacity = 40 }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges(); // Save students and courses first so their IDs are generated

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
        };
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

app.MapGet("/api/test/challenge1", () =>
{
    var results = new List<string>();

    try
    {
        // 1. Archiving a course zeroes its capacity
        var course = new Course
        {
            Code = "CS-101",
            Title = "C# Basics",
            Capacity = 10
        };

        course.Status = CourseStatus.Archived;

        results.Add(course.Capacity == 0
            ? "PASS: Archived course has zero capacity."
            : "FAIL: Capacity must drop to 0 when archived.");

        // 2. Empty name is rejected
        try
        {
            var badStudent = new Student(0, "TMS-0000", "");

            results.Add("FAIL: Empty name should throw.");
        }
        catch (ArgumentException)
        {
            results.Add("PASS: Empty name correctly rejected.");
        }

        // 3. Valid student
        var student = new Student(99, "TMS-0099", "Abeba");
        results.Add("PASS: Valid student created.");

        // 4. Invalid grade
        try
        {
            var badGrade = new GradeRecord(
                "CS-101",
                150m,
                DateTime.UtcNow);

            results.Add("FAIL: Invalid score should throw.");
        }
        catch (ArgumentOutOfRangeException)
        {
            results.Add("PASS: Invalid score correctly rejected.");
        }

        // 5. Value equality
        var grade1 = new GradeRecord(
            "CS-101",
            95.5m,
            DateTime.UnixEpoch);

        var grade2 = new GradeRecord(
            "CS-101",
            95.5m,
            DateTime.UnixEpoch);

        results.Add(grade1 == grade2
            ? "PASS: Value equality confirmed."
            : "FAIL: Record structs should compare by value.");

        results.Add("Challenge 1 PASSED");
    }
    catch (Exception ex)
    {
        results.Add($"Unexpected failure: {ex.Message}");
    }

    return Results.Ok(new
    {
        Challenge = "Module 1 - Challenge 1",
        Results = results
    });
});

app.Run();