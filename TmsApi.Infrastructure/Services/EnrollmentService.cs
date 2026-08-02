using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService( TmsDbContext context, ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public Task<bool> ExistsAsync(
    int studentId,
    string courseCode,
    CancellationToken ct)
{
    return context.Enrollments
        .Include(e => e.Course)
        .AnyAsync(
            e => e.StudentId == studentId &&
                 e.Course.Code == courseCode,
            ct);
}

public async Task AddAsync(
    Enrollment enrollment,
    CancellationToken ct)
{
    context.Enrollments.Add(enrollment);

    await context.SaveChangesAsync(ct);
}

public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(
    int studentId,
    CancellationToken ct)
{
    return await context.Enrollments
        .Include(e => e.Course)
        .Where(e => e.StudentId == studentId)
        .ToListAsync(ct);
}

public async Task<IReadOnlyList<Enrollment>> GetAllAsync(
    CancellationToken ct)
{
    return await context.Enrollments
        .Include(e => e.Student)
        .Include(e => e.Course)
        .AsNoTracking()
        .ToListAsync(ct);
}

public async Task ApproveAsync(
    int enrollmentId,
    CancellationToken ct)
{
    var enrollment = await context.Enrollments
        .FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);

    if (enrollment is null)
    {
        throw new KeyNotFoundException(
            $"Enrollment {enrollmentId} was not found.");
    }

    enrollment.Status = "Approved";

    await context.SaveChangesAsync(ct);
}

public Task<EnrollmentResponseDto?> GetByIdAsync( int courseId, int id, CancellationToken ct)
    {
        return context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);
    }
    public async Task<EnrollmentResponseDto> CreateAsync( int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation( "Student {StudentId} enrolled in course {CourseId}", request.StudentId, courseId);

        return (await GetByIdAsync( courseId, enrollment.Id, ct))!;
    }
    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync( int courseId, CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.StudentId,
                e.CourseId,
                e.EnrolledAt))
            .ToListAsync(ct);
    }
}