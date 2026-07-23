using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetAllCoursesHandler(
    ICourseService courseService)
    : IRequestHandler<GetAllCoursesQuery, List<CourseResponseDto>>
{
    public async Task<List<CourseResponseDto>> Handle(
        GetAllCoursesQuery request,
        CancellationToken ct)
    {
        var courses = await courseService.GetAllAsync(ct);

        return courses
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToList();
    }
}