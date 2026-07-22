namespace TmsApi.Application.Interfaces;

using TmsApi.Application.DTOs;

public interface ICachedCourseService
{
    Task<CourseResponseDto> GetCourseAsync(
        string code,
        CancellationToken ct);

    Task<List<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct);

    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}