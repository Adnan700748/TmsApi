using MediatR;

namespace TmsApi.Application.Courses.Commands;

public record DeleteCourseCommand(
    string Code) : IRequest<bool>;