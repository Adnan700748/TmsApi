using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(
    ICachedCourseService cachedCourseService,
    IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var allCourses = await cachedCourseService.GetAllCoursesAsync(ct);
        var totalCount = allCourses.Count;

        var rows = allCourses
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Code,
                c.MaxCapacity,
                EnrollmentCount = c.EnrollmentCount
            })
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        return Ok(new
        {
            data = rows,

            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext,
                hasPrevious
            },

            links = new
            {
                self = $"/api/v2/courses?page={page}&pageSize={pageSize}",

                next = hasNext ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}" : (string?)null,

                prev = hasPrevious ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}" : (string?)null,

                enroll = "/api/v2/enrollments"
            }
        });
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularCourses(
        [FromQuery] int count = 5,
        CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 20);
        var query = new GetPopularCoursesQuery(count);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        CreateCourseCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(
                nameof(GetCourse),
                new
                {
                    version = "2.0",
                    id = created.Id
                },
                created),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "duplicate_code" => StatusCodes.Status409Conflict,
                    "invalid_capacity" => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };

                return Problem(
                    statusCode: status,
                    title: "Course creation failed",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(
        int id,
        CancellationToken ct)
    {
        var course = await cachedCourseService.GetCourseAsync(id, ct);

        if (course is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Course not found",
                detail: $"Course with ID {id} was not found.",
                type: "https://tms.local/errors/course_not_found");
        }

        return Ok(course);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        UpdateCourseCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "ID mismatch",
                detail: "The ID in the URL does not match the ID in the request body.");
        }

        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            onSuccess: updated => Ok(updated),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    "invalid_capacity" => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };

                return Problem(
                    statusCode: status,
                    title: "Course update failed",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(
        int id,
        CancellationToken ct)
    {
        var command = new DeleteCourseCommand(id);
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            onSuccess: _ => NoContent(),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    "has_enrollments" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };

                return Problem(
                    statusCode: status,
                    title: "Course deletion failed",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }
}