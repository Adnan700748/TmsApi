using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetCourseByCodeHandler(
    ICachedCourseService cachedCourseService)
    : IRequestHandler<GetCourseByCodeQuery, CourseResponseDto?>
{
    public async Task<CourseResponseDto?> Handle(
    GetCourseByCodeQuery request,
    CancellationToken ct)
{
    return await cachedCourseService.GetCourseAsync(
        request.Code,
        ct);
}
}