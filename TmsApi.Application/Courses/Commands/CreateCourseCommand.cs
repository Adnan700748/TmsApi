using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Commands;

public record CreateCourseCommand(
    string Code,
    string Title,
    int MaxCapacity)
    : IRequest<CourseResponseDto>;