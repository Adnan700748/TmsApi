using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<CreateCourseCommand, Result<CourseCreated, CourseError>>
{
    public async Task<Result<CourseCreated, CourseError>> Handle(
        CreateCourseCommand command,
        CancellationToken ct)
    {
        var codeExists = await courseService.CodeExistsAsync(command.Code, ct);

        if (codeExists)
        {
            return Result<CourseCreated, CourseError>.Failure(
                CourseError.DuplicateCode(command.Code));
        }

        if (command.MaxCapacity <= 0)
        {
            return Result<CourseCreated, CourseError>.Failure(
                CourseError.InvalidCapacity(command.MaxCapacity));
        }

        var request = new CreateCourseRequest
        {
            Code = command.Code,
            Title = command.Title,
            MaxCapacity = command.MaxCapacity
        };

        var course = await courseService.CreateAsync(request, ct);

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return Result<CourseCreated, CourseError>.Success(
            new CourseCreated(
                course.Id,
                course.Code,
                course.Title,
                course.MaxCapacity));
    }
}