using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
  [HttpGet("active-students-count")]
public async Task<IActionResult> ActiveStudentsCount()
{
    var count = await context.Students
        .Where(s => s.IsActive && s.GPA >= 3.0m)
        .CountAsync();

    return Ok(count);
}

[HttpGet("courses-by-enrollment")]
public async Task<IActionResult> CoursesByEnrollment()
{
    var list = await context.Courses
        .Select(c => new
        {
            c.Title,
            EnrollmentCount = c.Enrollments.Count
        })
        .OrderByDescending(x => x.EnrollmentCount)
        .ToListAsync();

    return Ok(list);
}

[HttpGet("average-gpa-per-course")]
public async Task<IActionResult> AverageGpaPerCourse()
{
    var list = await context.Enrollments
        .GroupBy(e => e.Course.Title)
        .Select(g => new
        {
            Course = g.Key,
            AverageGPA = g.Average(e => e.Student.GPA)
        })
        .ToListAsync();

    return Ok(list);
}

[HttpGet("students-without-enrollments")]
public async Task<IActionResult> StudentsWithoutEnrollments()
{
    var list = await context.Students
        .Where(s => !s.Enrollments.Any())
        .Select(s => s.Name)
        .ToListAsync();

    return Ok(list);
}

[HttpGet("students-without-enrollments-leftjoin")]
public async Task<IActionResult> StudentsWithoutEnrollmentsLeftJoin()
{
    var list = await context.Students
        .LeftJoin(
            context.Enrollments,
            s => s.Id,
            e => e.StudentId,
            (s, e) => new { s, e })
        .Where(x => x.e == null)
        .Select(x => x.s.Name)
        .ToListAsync();

    return Ok(list);
}
}