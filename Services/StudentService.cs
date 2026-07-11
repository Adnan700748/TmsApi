using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Dtos;

public class StudentService(TmsDbContext db, ILogger<StudentService> logger) : IStudentService
{
    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
    PagedRequest request,
    CancellationToken ct)
{
    IQueryable<Student> query = db.Students.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(request.Search))
    {
        query = query.Where(s =>
            EF.Functions.ILike(
                s.Name,
                $"%{request.Search}%") ||
            EF.Functions.ILike(
                s.RegistrationNumber,
                $"%{request.Search}%"));
    }

    // Count BEFORE Skip/Take
    var totalCount = await query.CountAsync(ct);

    query = request.OrderBy.ToLowerInvariant() switch
    {
        "registrationnumber" => request.Descending
            ? query.OrderByDescending(s => s.RegistrationNumber)
            : query.OrderBy(s => s.RegistrationNumber),

        "gpa" => request.Descending
            ? query.OrderByDescending(s => s.GPA)
            : query.OrderBy(s => s.GPA),

        _ => request.Descending
            ? query.OrderByDescending(s => s.Name)
            : query.OrderBy(s => s.Name)
    };

    var items = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(s => new StudentResponseDto(
            s.Id,
            s.RegistrationNumber,
            s.Name,
            s.GPA,
            s.IsActive))
        .ToListAsync(ct);

    return new PagedResponse<StudentResponseDto>
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
}
    public async Task<StudentResponseDto?> GetByIdAsync( int id, CancellationToken ct)
    {
        return await db.Students
        .AsNoTracking()
        .Where(s => s.Id == id)
        .Select(s => new StudentResponseDto(
            s.Id,
            s.RegistrationNumber,
            s.Name,
            s.GPA,
            s.IsActive))
        .FirstOrDefaultAsync(ct); 
        }
    public async Task<StudentResponseDto> AddAsync(CreateStudentRequest request)
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

    public async Task<StudentResponseDto?> GetByIdAsync(string id)
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

    public async Task<IReadOnlyList<StudentResponseDto>> GetAllAsync()
    {
        return await db.Students
            .Select(s => new StudentResponseDto(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.IsActive))
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


    private static StudentResponseDto ToResponse(Student s) =>
        new(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.IsActive);

        public async Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentRequest request)
{
    // 1. Find the student
    var student = await db.Students.FindAsync(id);
    if (student is null)
    {
        logger.LogWarning("Update failed: Student {StudentId} not found", id);
        return null;
    }

    // 2. Update only if values are provided
    if (!string.IsNullOrEmpty(request.Name))
        student.Name = request.Name;
    
    if (request.GPA.HasValue)
        student.GPA = request.GPA.Value;
    
    if (request.IsActive.HasValue)
        student.IsActive = request.IsActive.Value;

    // 3. Save changes
    await db.SaveChangesAsync();
    logger.LogInformation("Updated student {StudentId}", id);
    
    return ToResponse(student);
}
}