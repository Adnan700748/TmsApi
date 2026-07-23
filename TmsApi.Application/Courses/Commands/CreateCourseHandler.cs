using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseHandler(
    ICourseService courseService)
    : IRequestHandler<CreateCourseCommand, CourseResponseDto>
{
    public async Task<CourseResponseDto> Handle(
        CreateCourseCommand request,
        CancellationToken ct)
    {
        var createRequest = new CreateCourseRequest
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        return await courseService.CreateAsync(
            createRequest,
            ct);
    }
}