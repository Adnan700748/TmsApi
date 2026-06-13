public interface ICourseService
{
    Task<Course> CreateAsync(Course course);
    Task<Course?> GetByCodeAsync(string code);
    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    private readonly Dictionary<string, Course> _store = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<Course> CreateAsync(Course course)
    {
        if (_store.ContainsKey(course.Code))
        {
            _logger.LogWarning(
                "Duplicate course creation attempt {CourseCode}",
                course.Code);

            return Task.FromResult(_store[course.Code]);
        }

        _store[course.Code] = course;

        _logger.LogInformation(
            "Created course {CourseCode}",
            course.Code);

        return Task.FromResult(course);
    }

    public Task<Course?> GetByCodeAsync(string code)
    {
        _store.TryGetValue(code, out var course);

        if (course is null)
        {
            _logger.LogWarning(
                "Course {CourseCode} not found",
                code);
        }

        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<Course>> GetAllAsync()
    {
        IReadOnlyList<Course> courses = _store.Values.ToList();

        return Task.FromResult(courses);
    }

    public Task<bool> DeleteAsync(string code)
    {
        var removed = _store.Remove(code);

        if (removed)
        {
            _logger.LogInformation(
                "Deleted course {CourseCode}",
                code);
        }
        else
        {
            _logger.LogWarning(
                "Delete failed course {CourseCode} not found",
                code);
        }

        return Task.FromResult(removed);
    }
}
