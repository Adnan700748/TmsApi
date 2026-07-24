using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;
using TmsApi.Application.DTOs;
using Microsoft.AspNetCore.RateLimiting;

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

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(
        string code,
        UpdateCourseRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCourseCommand(
            code,
            request.Title,
            request.MaxCapacity);

        var updated = await mediator.Send(command, ct);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(
        string code,
        CancellationToken ct)
    {
        var deleted = await mediator.Send(
            new DeleteCourseCommand(code),
            ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
    [HttpGet("search")]
[EnableRateLimiting("search")]
public async Task<IActionResult> SearchCourses(
    [FromQuery] string? term,
    CancellationToken ct)
{
    var results = await mediator.Send(
        new SearchCoursesQuery(term),
        ct);

    return Ok(results);
}
}