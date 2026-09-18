using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class AssessmentService(
    TmsDbContext context,
    ILogger<AssessmentService> logger) : IAssessmentService
{
    public Task<AssessmentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return context.Assessments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AssessmentResponseDto> CreateAsync(
        CreateAssessmentRequest request,
        CancellationToken ct)
    {
        var assessment = new Assessment
        {
            Title = request.Title,
            MaxScore = request.MaxScore,
            Weight = request.Weight,
            CourseId = request.CourseId
        };

        context.Assessments.Add(assessment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created assessment {AssessmentId} ({AssessmentTitle})",
            assessment.Id,
            assessment.Title);

        return (await GetByIdAsync(assessment.Id, ct))!;
    }

    public async Task<PagedResponse<AssessmentResponseDto>> GetAssessmentsAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        IQueryable<Assessment> query =
            context.Assessments.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Title,
                    $"%{request.Search}%"));
        }

        // Count BEFORE paging
        var totalCount = await query.CountAsync(ct);

        // Sorting
        query = request.OrderBy switch
        {
            "MaxScore" => request.Descending
                ? query.OrderByDescending(a => a.MaxScore)
                : query.OrderBy(a => a.MaxScore),

            "Weight" => request.Descending
                ? query.OrderByDescending(a => a.Weight)
                : query.OrderBy(a => a.Weight),

            "CourseId" => request.Descending
                ? query.OrderByDescending(a => a.CourseId)
                : query.OrderBy(a => a.CourseId),

            _ => request.Descending
                ? query.OrderByDescending(a => a.Title)
                : query.OrderBy(a => a.Title)
        };

        // Paging + Projection
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .ToListAsync(ct);

        return new PagedResponse<AssessmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}