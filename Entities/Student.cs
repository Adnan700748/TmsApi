namespace TmsApi.Entities;
using System.Diagnostics.CodeAnalysis;
public class Student 
{ 
    public int Id { get; set; } 
    public required string RegistrationNumber {get; set; } 
    public required string Name {get; set; } 
    public int Age { get; set; }
    public decimal GPA {get; set; }
    public bool IsActive {get; set; } = true;
    public uint Version { get; set; }
    public ICollection<Enrollment> Enrollments {get; set; } = new List<Enrollment>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    private readonly List<GradeRecord> _grades = new();
    public IReadOnlyList<GradeRecord> Grades => _grades.AsReadOnly();

[SetsRequiredMembers]
public Student(int id, string registrationNumber, string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Name cannot be empty.", nameof(name));

    if (string.IsNullOrWhiteSpace(registrationNumber))
        throw new ArgumentException(
            "Registration number cannot be empty.",
            nameof(registrationNumber));

    Id = id;
    RegistrationNumber = registrationNumber;
    Name = name;
}
[SetsRequiredMembers]
public Student()
{
    RegistrationNumber = string.Empty;
    Name = string.Empty;
}
public void AddGrade(GradeRecord grade)
{
    _grades.Add(grade);
}
   
}
