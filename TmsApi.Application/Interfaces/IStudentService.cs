using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken ct);

    Task<StudentResponseDto?> UpdateAsync(
        int id,
        UpdateStudentRequest request,
        CancellationToken ct);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken ct);

    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        PagedRequest request,
        CancellationToken ct);

    Task<IReadOnlyList<EnrollmentResponseDto>> GetEnrollmentsAsync(
        int studentId,
        CancellationToken ct);
}