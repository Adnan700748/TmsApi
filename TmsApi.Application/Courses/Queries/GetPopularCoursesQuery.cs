using MediatR;

namespace TmsApi.Application.Courses.Queries;

public record GetPopularCoursesQuery(int Count = 5) : IRequest<List<PopularCourseDto>>;

public record PopularCourseDto(
    int CourseId,
    string Code,
    string Title,
    int EnrollmentCount,
    double PopularityScore);