using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Courses.Commands;

public record DeleteCourseCommand(int Id) : IRequest<Result<bool, CourseError>>;