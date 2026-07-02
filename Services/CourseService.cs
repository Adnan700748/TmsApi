using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger)
    : ICourseService
{
    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        throw new NotImplementedException();
    }

    public async Task<Course> CreateAsync(Course course, CancellationToken ct)
    {
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Course {Code} created successfully.",course.Code);
        return course;
        throw new NotImplementedException();
    }
}