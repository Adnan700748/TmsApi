using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var courses = await mediator.Send(
            new GetAllCoursesQuery(),
            ct);

        return Ok(courses);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(
        string code,
        CancellationToken ct)
    {
        var course = await mediator.Send(
            new GetCourseByCodeQuery(code),
            ct);

        if (course is null)
            return NotFound();

        return Ok(course);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCourseCommand command,
        CancellationToken ct)
    {
        var course = await mediator.Send(command, ct);

        return CreatedAtAction(
            nameof(GetByCode),
            new { code = course.Code },
            course);
    }
}