using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetPopularCoursesHandler(
    ICachedCourseService cachedCourseService)
    : IRequestHandler<GetPopularCoursesQuery, List<PopularCourseDto>>
{
    public async Task<List<PopularCourseDto>> Handle(
        GetPopularCoursesQuery query,
        CancellationToken ct)
    {
        var courses = await cachedCourseService.GetAllCoursesAsync(ct);

        var popular = courses
            .Where(c => c.EnrollmentCount > 0)
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(query.Count)
            .Select(c => new PopularCourseDto(
                c.Id,
                c.Code,
                c.Title,
                c.EnrollmentCount,
                Math.Min(100, (c.EnrollmentCount / (double)Math.Max(1, c.MaxCapacity)) * 100)))
            .ToList();

        return popular;
    }
}