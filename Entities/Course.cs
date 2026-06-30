namespace TmsApi.Entities;
using TmsApi.Enums;

public class Course
{
    public required string Code { get; init; }
    public required string Title { get; init; }
    public int Capacity { get; set; }
    
    private CourseStatus _status = CourseStatus.Active;
    
    public CourseStatus Status
{
    get;
    set
    {
        field = value;

        if (value == CourseStatus.Archived)
        {
            Capacity = 0;
        }
    }
} = CourseStatus.Active;
    
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}