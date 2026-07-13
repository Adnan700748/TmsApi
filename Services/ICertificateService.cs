using TmsApi.Dtos;

namespace TmsApi.Services;

public interface ICertificateService
{
  Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync( PagedRequest request, CancellationToken ct);
  Task<CertificateResponseDto?> GetByIdAsync( int id, CancellationToken ct);
  Task<bool> SerialNumberExistsAsync( string serialNumber, CancellationToken ct);
  Task<CertificateResponseDto> CreateAsync( CreateCertificateRequest request, CancellationToken ct);
}