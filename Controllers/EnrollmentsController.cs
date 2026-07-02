using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController( ICourseService courseService, IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment( int courseId, int id, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync( courseId, id, ct);
        return enrollment is null ? NotFound(): Ok(enrollment);
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent( int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail =$"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var enrollment = await enrollmentService.CreateAsync( courseId, request, ct);
        return CreatedAtAction( nameof(GetEnrollment),new { courseId, id = enrollment.Id }, enrollment);
    }
}