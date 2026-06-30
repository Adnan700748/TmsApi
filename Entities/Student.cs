namespace TmsApi.Entities;
public class Student 
{ 
    public int Id { get; set; } 
    public required string RegistrationNumber {get; set; } 
    public required string Name {get; set; } 
    public decimal GPA {get; set; }
    public bool IsActive {get; set; } = true;
    public uint Version { get; set; }
    public ICollection<Enrollment> Enrollments {get; set; } = new List<Enrollment>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    private readonly List<GradeRecord> _grades = new();
    public IReadOnlyList<GradeRecord> Grades => _grades.AsReadOnly();
    public Student(int id, string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Name cannot be empty.", nameof(name));

    Id = id;
    Name = name;
}
public Student()
{
}
   
}
