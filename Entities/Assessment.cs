namespace TmsApi.Entities;
public class Assessment 
{ 
    public int Id { get; set; } 
    public required string Titel {get; set; } 
    public decimal MaxScore {get; set; }
    public decimal Weight {get; set; }
   
   // Foreign key + navigation to the owning course
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
