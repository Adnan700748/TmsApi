using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<UpdateCourseCommand, Result<CourseUpdated, CourseError>>
{
    public async Task<Result<CourseUpdated, CourseError>> Handle(
        UpdateCourseCommand command,
        CancellationToken ct)
    {
        var existing = await courseService.GetByIdAsync(command.Id, ct);

        if (existing is null)
        {
            return Result<CourseUpdated, CourseError>.Failure(
                CourseError.NotFound(command.Id));
        }

        if (command.MaxCapacity <= 0)
        {
            return Result<CourseUpdated, CourseError>.Failure(
                CourseError.InvalidCapacity(command.MaxCapacity));
        }

        var course = await courseService.GetByCodeAsync(existing.Code, ct);

        if (course is null)
        {
            return Result<CourseUpdated, CourseError>.Failure(
                CourseError.NotFoundByCode(existing.Code));
        }

        course.Title = command.Title;
        course.MaxCapacity = command.MaxCapacity;

        await courseService.UpdateAsync(course, ct);

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return Result<CourseUpdated, CourseError>.Success(
            new CourseUpdated(
                course.Id,
                course.Code,
                course.Title,
                course.MaxCapacity));
    }
}