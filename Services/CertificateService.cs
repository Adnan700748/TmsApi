using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CertificateService( TmsDbContext context, ILogger<CertificateService> logger) : ICertificateService
{
    public Task<CertificateResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return context.Certificates
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.CourseId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CertificateResponseDto> CreateAsync(
        CreateCertificateRequest request,
        CancellationToken ct)
    {
        var certificate = new Certificate
        {
            SerialNumber = request.SerialNumber,
            StudentId = request.StudentId,
            CourseId = request.CourseId
        };

        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created certificate {CertificateId} ({SerialNumber})",
            certificate.Id,
            certificate.SerialNumber);

        return (await GetByIdAsync(certificate.Id, ct))!;
    }

    public Task<bool> SerialNumberExistsAsync(
        string serialNumber,
        CancellationToken ct)
    {
        return context.Certificates
            .AsNoTracking()
            .AnyAsync(c => c.SerialNumber == serialNumber, ct);
    }

    public async Task<PagedResponse<CertificateResponseDto>>
        GetCertificatesAsync(
            PagedRequest request,
            CancellationToken ct)
    {
        IQueryable<Certificate> query =
            context.Certificates.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(
                    c.SerialNumber,
                    $"%{request.Search}%"));
        }

        // Count BEFORE paging
        var totalCount = await query.CountAsync(ct);

        // Sorting
        query = request.OrderBy switch
        {
            "SerialNumber" => request.Descending
                ? query.OrderByDescending(c => c.SerialNumber)
                : query.OrderBy(c => c.SerialNumber),

            "IssuedAt" => request.Descending
                ? query.OrderByDescending(c => c.IssuedAt)
                : query.OrderBy(c => c.IssuedAt),

            _ => request.Descending
                ? query.OrderByDescending(c => c.Id)
                : query.OrderBy(c => c.Id)
        };

        // Paging + Projection
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.CourseId))
            .ToListAsync(ct);

        return new PagedResponse<CertificateResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}