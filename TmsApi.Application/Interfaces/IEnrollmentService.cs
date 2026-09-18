using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync( int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync( int courseId, EnrollStudentRequest request, CancellationToken ct);
    //Task<List<EnrollmentResponseDto>> GetByCourseAsync( int courseId, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync( int courseId, CancellationToken ct);
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);
    Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);
    Task<IReadOnlyList<Enrollment>> GetAllAsync(CancellationToken ct);
    //Task ApproveAsync(int enrollmentId, CancellationToken ct);
   Task ApproveAsync(
    int enrollmentId,
    CancellationToken ct);

Task RejectAsync(
    int enrollmentId,
    CancellationToken ct);}