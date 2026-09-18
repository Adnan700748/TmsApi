using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Queries;

public record GetAllCoursesQuery()
    : IRequest<List<CourseResponseDto>>;