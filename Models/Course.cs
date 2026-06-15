public class Course 
{ 
    public required string Code { get; init; } 
 
    public required string Title 
    { 
        get; 
        set => field = !string.IsNullOrWhiteSpace(value) 
            ? value 
            : throw new ArgumentException("Title cannot be empty or whitespace.", nameof(value)); 
    } 
    public int Capacity 
    { 
        get; 
        set => field = value > 0 
            ? value 
            : throw new ArgumentOutOfRangeException(nameof(value), "System constraint: Capacity must be greater than zero."); 
    } 
 
    public int EnrolledCount { get; set; } 
} 