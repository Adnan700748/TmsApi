using TmsApi.Dtos;

namespace TmsApi.Services;

public interface IAssessmentService
{
    Task<AssessmentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<AssessmentResponseDto> CreateAsync(
        CreateAssessmentRequest request,
        CancellationToken ct);

    Task<PagedResponse<AssessmentResponseDto>> GetAssessmentsAsync(
        PagedRequest request,
        CancellationToken ct);
}