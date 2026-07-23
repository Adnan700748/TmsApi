using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Courses.Commands;

public record UpdateCourseCommand(int Id, string Title, int MaxCapacity) : IRequest<Result<CourseUpdated, CourseError>>;

public record CourseUpdated(int Id, string Code, string Title, int MaxCapacity);