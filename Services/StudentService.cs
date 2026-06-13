public interface IStudentService
{
    Task<Student> CreateAsync(Student student);
    Task<Student?> GetByIdAsync(string id);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class StudentService : IStudentService
{
    private readonly Dictionary<string, Student> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<Student> CreateAsync(Student student)
    {
        if (_store.ContainsKey(student.Id))
        {
            _logger.LogWarning(
                "Duplicate student creation attempt {StudentId}",
                student.Id);

            return Task.FromResult(_store[student.Id]);
        }

        _store[student.Id] = student;

        _logger.LogInformation(
            "Created student {StudentId}",
            student.Id);

        return Task.FromResult(student);
    }

    public Task<Student?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var student);

        if (student is null)
        {
            _logger.LogWarning(
                "Student {StudentId} not found",
                id);
        }

        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync()
    {
        IReadOnlyList<Student> students = _store.Values.ToList();

        return Task.FromResult(students);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);

        if (removed)
        {
            _logger.LogInformation(
                "Deleted student {StudentId}",
                id);
        }
        else
        {
            _logger.LogWarning(
                "Delete failed student {StudentId} not found",
                id);
        }

        return Task.FromResult(removed);
    }
}