using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class DeleteCourseHandler(
    ICourseService service,
    ICachedCourseService cachedService)
    : IRequestHandler<DeleteCourseCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCourseCommand command,
        CancellationToken ct)
    {
        var course = await service.GetByCodeAsync(command.Code, ct);

        if (course is null)
            return false;

        var deleted = await service.DeleteAsync(course, ct);

        if (!deleted)
            return false;

        await cachedService.InvalidateCourseCacheAsync(ct);

        return true;
    }
}