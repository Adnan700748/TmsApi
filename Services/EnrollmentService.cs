using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public record EnrollmentResponse(int Id, int StudentId, int CourseId, decimal? Grade, DateTime EnrolledAt);

public interface IEnrollmentService
{
    Task<EnrollmentResponse> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentResponse?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentResponse>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class EnrollmentService(TmsDbContext db, ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public async Task<EnrollmentResponse> EnrollAsync(string studentId, string courseCode)
    {
        // Resolve student by id or registration number
        Student? student = int.TryParse(studentId, out var sid)
            ? await db.Students.FindAsync(sid)
            : await db.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == studentId);

        // Resolve course by id or code
        Course? course = int.TryParse(courseCode, out var cid)
            ? await db.Courses.FindAsync(cid)
            : await db.Courses.FirstOrDefaultAsync(c => c.Code == courseCode);

        if (student is null || course is null)
        {
            logger.LogWarning("Enrollment failed: student '{StudentId}' or course '{CourseCode}' not found",
                studentId, courseCode);
            throw new InvalidOperationException($"Student '{studentId}' or course '{courseCode}' not found.");
        }

        var existing = await db.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == course.Id);

        if (existing is not null)
        {
            logger.LogWarning("Duplicate enrollment: student {StudentId} already in course {CourseId}",
                student.Id, course.Id);
            return ToResponse(existing);
        }

        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();
        logger.LogInformation("Enrolled student {StudentId} in course {CourseId}, record {EnrollmentId}",
            student.Id, course.Id, enrollment.Id);
        return ToResponse(enrollment);
    }

    public async Task<EnrollmentResponse?> GetByIdAsync(string id)
    {
        if (!int.TryParse(id, out var intId))
        {
            logger.LogWarning("Enrollment {EnrollmentId} not found", id);
            return null;
        }
        var enrollment = await db.Enrollments.FindAsync(intId);
        if (enrollment is null)
        {
            logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        }
        return enrollment is null ? null : ToResponse(enrollment);
    }

    public async Task<IReadOnlyList<EnrollmentResponse>> GetAllAsync()
    {
        return await db.Enrollments
            .Select(e => new EnrollmentResponse(e.Id, e.StudentId, e.CourseId, e.Grade, e.EnrolledAt))
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out var intId))
        {
            logger.LogWarning("Delete failed: enrollment {EnrollmentId} not found", id);
            return false;
        }
        var enrollment = await db.Enrollments.FindAsync(intId);
        if (enrollment is null)
        {
            logger.LogWarning("Delete failed: enrollment {EnrollmentId} not found", id);
            return false;
        }
        db.Enrollments.Remove(enrollment);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        return true;
    }

    private static EnrollmentResponse ToResponse(Enrollment e) =>
        new(e.Id, e.StudentId, e.CourseId, e.Grade, e.EnrolledAt);
}

public class TmsDatabaseException(string message) : Exception(message);