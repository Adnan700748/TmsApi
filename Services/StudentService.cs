using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public interface IStudentService
{
    Task<Student> CreateAsync(Student student);
    Task<Student?> GetByIdAsync(int id);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
}

public class StudentService : IStudentService
{
    private readonly TmsDbContext _dbContext;
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger, TmsDbContext dbContext)

    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Student> CreateAsync(Student student)
    {
        var existing = await _dbContext.Students.Where(s => s.Id == student.Id).AnyAsync();

        if (existing)
        {
            _logger.LogWarning(
                "Duplicate student creation attempt {StudentId}",
                student.Id);

            return student;
        }

        _dbContext.Students.Add(student);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created student {StudentId}",
            student.Id);

        return student;
    }
    public async Task<Student?> GetByIdAsync(int id)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student is null)
        {
            _logger.LogWarning(
                "Student {StudentId} not found",
                id);
        }

        return student;
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        return await _dbContext.Students.ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student is null)
        {
            _logger.LogWarning(
                "Delete failed student {StudentId} not found",
                id);

            return false;
        }

        _dbContext.Students.Remove(student);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted student {StudentId}",
            id);

        return true;
    }
}