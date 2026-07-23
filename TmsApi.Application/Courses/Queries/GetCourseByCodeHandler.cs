using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetCourseByCodeHandler(
    ICourseService courseService)
    : IRequestHandler<GetCourseByCodeQuery, CourseResponseDto?>
{
    public async Task<CourseResponseDto?> Handle(
        GetCourseByCodeQuery request,
        CancellationToken ct)
    {
        var course = await courseService.GetByCodeAsync(
            request.Code,
            ct);

        if (course is null)
            return null;

        return new CourseResponseDto(
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity,
            course.Enrollments.Count);
    }
}