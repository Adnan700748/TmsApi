using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CertificateService(TmsDbContext db) : ICertificateService
{
    public async Task<PagedResponse<CertificateResponseDto>>
        GetCertificatesAsync(
            PagedRequest request,
            CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var query = db.Certificates
            .AsNoTracking()
            .AsQueryable();

        // Filter first
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(c =>
                c.SerialNumber.Contains(search));
        }

        // Count after filtering
        var totalCount = await query.CountAsync(ct);

        // Then page
        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.CourseId))
            .ToListAsync(ct);

        return new PagedResponse<CertificateResponseDto>(
            items,
            totalCount,
            page,
            pageSize);
    }

    public async Task<CertificateResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await db.Certificates
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

    public async Task<bool> SerialNumberExistsAsync(
        string serialNumber,
        CancellationToken ct)
    {
        return await db.Certificates
            .AnyAsync(
                c => c.SerialNumber == serialNumber,
                ct);
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

        db.Certificates.Add(certificate);
        await db.SaveChangesAsync(ct);

        return new CertificateResponseDto(
            certificate.Id,
            certificate.SerialNumber,
            certificate.IssuedAt,
            certificate.StudentId,
            certificate.CourseId);
    }
}