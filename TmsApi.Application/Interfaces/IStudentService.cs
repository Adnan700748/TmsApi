using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDto> AddAsync(CreateStudentRequest request);
    Task<StudentResponseDto?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentRequest request);
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync( PagedRequest request, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetEnrollmentsAsync(
        int studentId,
        CancellationToken ct);

}