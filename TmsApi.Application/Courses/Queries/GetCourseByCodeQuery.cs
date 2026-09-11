using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Queries;

public record GetCourseByCodeQuery(string Code)
    : IRequest<CourseResponseDto?>;