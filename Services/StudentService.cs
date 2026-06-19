using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public record CreateStudentRequest(string RegistrationNumber, string Name, decimal GPA, bool IsActive = true);
public record StudentResponse(int Id, string RegistrationNumber, string Name, decimal GPA, bool IsActive);

public interface IStudentService
{
    Task<StudentResponse> AddAsync(CreateStudentRequest request);
    Task<StudentResponse?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentResponse>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class StudentService(TmsDbContext db, ILogger<StudentService> logger) : IStudentService
{
    public async Task<StudentResponse> AddAsync(CreateStudentRequest request)
    {
        var existing = await db.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == request.RegistrationNumber);
        if (existing is not null)
        {
            logger.LogWarning("Student {RegistrationNumber} already exists", request.RegistrationNumber);
            return ToResponse(existing);
        }

        var student = new Student
        {
            RegistrationNumber = request.RegistrationNumber,
            Name = request.Name,
            GPA = request.GPA,
            IsActive = request.IsActive
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();
        logger.LogInformation("Added student {RegistrationNumber}", student.RegistrationNumber);
        return ToResponse(student);
    }

    public async Task<StudentResponse?> GetByIdAsync(string id)
    {
        // Support lookup by registration number or numeric id
        Student? student = int.TryParse(id, out var intId)
            ? await db.Students.FindAsync(intId)
            : await db.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == id);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found", id);
            return null;
        }
        return ToResponse(student);
    }

    public async Task<IReadOnlyList<StudentResponse>> GetAllAsync()
    {
        return await db.Students
            .Select(s => new StudentResponse(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.IsActive))
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        Student? student = int.TryParse(id, out var intId)
            ? await db.Students.FindAsync(intId)
            : await db.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == id);

        if (student is null)
        {
            logger.LogWarning("Delete failed: Student {StudentId} not found", id);
            return false;
        }
        db.Students.Remove(student);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted student {StudentId}", id);
        return true;
    }

    private static StudentResponse ToResponse(Student s) =>
        new(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.IsActive);
}