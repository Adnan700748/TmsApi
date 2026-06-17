using System;
namespace TmsApi.Entities;
public class Enrollment
{
    public int Id { get; set; }
    public string? StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; } 
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation properties back to entities
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}