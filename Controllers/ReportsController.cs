using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
    // Query 1: How many active students have GPA >= 3.0?
    // SQL: SELECT COUNT(*)::int FROM "Students" AS s WHERE s."IsActive" AND s."GPA" >= 3.0
    [HttpGet("active-high-gpa-count")]
    public async Task<IActionResult> GetActiveHighGpaCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();
        return Ok(new { ActiveStudentsWithGpaAbove3 = count });
    }

    // Query 2: Which courses have the most enrollments, sorted descending?
    // SQL: ORDER BY and COUNT calculated in the database
    [HttpGet("courses-by-enrollment")]
    public async Task<IActionResult> GetCoursesByEnrollment()
    {
        var list = await context.Courses
            .Select(c => new { c.Title, EnrollmentCount = c.Enrollments.Count })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();
        return Ok(list);
    }

    // Query 3: What is the average GPA per course?
    // SQL: GROUP BY with AVG aggregation in the database
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> GetAverageGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new { Course = g.Key, AverageGPA = g.Average(e => e.Student.GPA) })
            .ToListAsync();
        return Ok(list);
    }

    // Query 4A: Which students have zero enrollments? (subquery approach)
    // SQL: NOT EXISTS (SELECT 1 FROM "Enrollments" WHERE ...)
    [HttpGet("unenrolled-students-subquery")]
    public async Task<IActionResult> GetUnenrolledStudentsSubquery()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();
        return Ok(list);
    }

    // Query 4B: Which students have zero enrollments? (EF Core 10 LeftJoin approach)
    // SQL: LEFT JOIN "Enrollments" ... WHERE e."Id" IS NULL
    [HttpGet("unenrolled-students-leftjoin")]
    public async Task<IActionResult> GetUnenrolledStudentsLeftJoin()
    {
        var list = await context.Students
            .LeftJoin(context.Enrollments, s => s.Id, e => e.StudentId, (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();
        return Ok(list);
    }

//student pagination
     [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var students = await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

//top5 courses
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses(
    CancellationToken cancellationToken = default)
    {
    var courses = await context.Enrollments
    .GroupBy(e => e.Course.Title)
    .Select(g => new
    {
        Title = g.Key,
        Count = g.Count()
    })
    .OrderByDescending(x => x.Count)
    .Take(5)
    .ToListAsync();

    return Ok(courses);
}



}