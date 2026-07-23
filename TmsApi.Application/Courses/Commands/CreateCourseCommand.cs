using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Courses.Commands;

public record CreateCourseCommand(string Code, string Title, int MaxCapacity) : IRequest<Result<CourseCreated, CourseError>>;

public record CourseCreated(int Id, string Code, string Title, int MaxCapacity);