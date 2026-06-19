using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public record CreateCourseRequest(string Code, string Title, int Capacity);
public record CourseResponse(int Id, string Code, string Title, int Capacity);

public interface ICourseService
{
    Task<CourseResponse> AddAsync(CreateCourseRequest request);
    Task<CourseResponse?> GetByIdAsync(string code);
    Task<IReadOnlyList<CourseResponse>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService(TmsDbContext db, ILogger<CourseService> logger) : ICourseService
{
    public async Task<CourseResponse> AddAsync(CreateCourseRequest request)
    {
        var existing = await db.Courses.FirstOrDefaultAsync(c => c.Code == request.Code);
        if (existing is not null)
        {
            logger.LogWarning("Course {CourseCode} already exists", request.Code);
            return ToResponse(existing);
        }

        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            Capacity = request.Capacity
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        logger.LogInformation("Added course {CourseCode}", course.Code);
        return ToResponse(course);
    }

    public async Task<CourseResponse?> GetByIdAsync(string code)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Code == code);
        if (course is null)
        {
            logger.LogWarning("Course {CourseCode} not found", code);
            return null;
        }
        return ToResponse(course);
    }

    public async Task<IReadOnlyList<CourseResponse>> GetAllAsync()
    {
        return await db.Courses
            .Select(c => new CourseResponse(c.Id, c.Code, c.Title, c.Capacity))
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(string code)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Code == code);
        if (course is null)
        {
            logger.LogWarning("Delete failed: Course {CourseCode} not found", code);
            return false;
        }
        db.Courses.Remove(course);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted course {CourseCode}", code);
        return true;
    }

    private static CourseResponse ToResponse(Course c) => new(c.Id, c.Code, c.Title, c.Capacity);
}