using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetAllCoursesHandler(
    ICachedCourseService cachedCourseService)
    : IRequestHandler<GetAllCoursesQuery, List<CourseResponseDto>>
{
    public async Task<List<CourseResponseDto>> Handle(
    GetAllCoursesQuery request,
    CancellationToken ct)
{
    return await cachedCourseService.GetAllCoursesAsync(ct);
}
}