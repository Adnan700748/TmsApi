namespace TmsApi.Entities;
public class Course 
{ 
    public int Id { get; init; } 
    public required string Code { get; set; } 
 
    public required string Title {get; set; } 
    public int Capacity { get; set; } 
    public ICollection<Enrollment> Enrollments {get; set; } = new List<Enrollment>();
} 