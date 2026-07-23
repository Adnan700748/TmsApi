using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class DeleteCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<DeleteCourseCommand, Result<bool, CourseError>>
{
    public async Task<Result<bool, CourseError>> Handle(
        DeleteCourseCommand command,
        CancellationToken ct)
    {
        var existing = await courseService.GetByIdAsync(command.Id, ct);

        if (existing is null)
        {
            return Result<bool, CourseError>.Failure(
                CourseError.NotFound(command.Id));
        }

        var course = await courseService.GetByCodeAsync(existing.Code, ct);

        if (course is null)
        {
            return Result<bool, CourseError>.Failure(
                CourseError.NotFoundByCode(existing.Code));
        }

        if (course.Enrollments.Any())
        {
            return Result<bool, CourseError>.Failure(
                CourseError.HasEnrollments(command.Id));
        }

        await courseService.DeleteAsync(course, ct);

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return Result<bool, CourseError>.Success(true);
    }
}