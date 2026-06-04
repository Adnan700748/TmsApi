using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions,
        TrainingAuthHandler> ("Training", null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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

app.Run();