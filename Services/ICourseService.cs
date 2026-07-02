using TmsApi.Entities;

namespace TmsApi.Services;

public interface ICourseService
{
    Task<Course?> GetByIdAsync(int id, CancellationToken ct);
    Task<Course> CreateAsync(Course course, CancellationToken ct);
}