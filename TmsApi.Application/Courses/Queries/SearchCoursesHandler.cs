using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class SearchCoursesHandler(ICourseService service)
    : IRequestHandler<SearchCoursesQuery, IReadOnlyList<CourseResponseDto>>
{
    public async Task<IReadOnlyList<CourseResponseDto>> Handle(
        SearchCoursesQuery request,
        CancellationToken ct)
    {
        var paged = await service.GetCoursesAsync(
            new PagedRequest
            {
                Search = request.Term,
                Page = 1,
                PageSize = 50
            },
            ct);

        return paged.Items;
    }
}