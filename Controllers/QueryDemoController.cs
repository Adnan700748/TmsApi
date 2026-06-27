using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryDemoController : ControllerBase
{
    private readonly TmsDbContext _context;
    private readonly ILogger<QueryDemoController> _logger;

    public QueryDemoController(TmsDbContext context, ILogger<QueryDemoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // PART A: N+1 Query - BAD
    [HttpGet("n-plus-one")]
    public async Task<IActionResult> GetStudentsWithEnrollmentCountNPlusOne()
    {
        _logger.LogInformation("=== EXERCISE 7 PART A: N+1 QUERY ===");
        
        // This causes N+1 problem
        var students = await _context.Students
            .AsNoTracking()
            .ToListAsync();

        var results = new List<object>();
        
        foreach (var student in students)
        {
            // Each iteration executes a separate COUNT query!
            var enrollmentCount = await _context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == student.Id);
                
            results.Add(new
            {
                student.RegistrationNumber,
                student.Name,
                student.GPA,
                EnrollmentCount = enrollmentCount
            });
            
            _logger.LogInformation($"Student {student.Name} has {enrollmentCount} enrollments");
        }

        _logger.LogInformation("=== TOTAL QUERIES: 1 + N = {Count} ===", students.Count + 1);
        
        return Ok(new
        {
            Message = "N+1 Query Demo - Check your console logs!",
            Students = results,
            TotalQueries = students.Count + 1,
            Note = "1 query for students + N queries for counts"
        });
    }

    // PART B: FIXED with Shaping
    [HttpGet("fixed")]
    public async Task<IActionResult> GetStudentsWithEnrollmentCountFixed()
    {
        _logger.LogInformation("=== EXERCISE 7 PART B: FIXED QUERY ===");
        
        // FIX: Single query with projection
        var results = await _context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync();

        foreach (var result in results)
        {
            _logger.LogInformation($"Student {result.Name} has {result.EnrollmentCount} enrollments");
        }

        _logger.LogInformation("=== TOTAL QUERIES: 1 (with subquery) ===");
        
        return Ok(new
        {
            Message = "Fixed Query - Only 1 SQL statement!",
            Students = results,
            TotalQueries = 1,
            Note = "Single query with subquery using EF Core projection"
        });
    }

    // Alternative: Using Include (loads full objects)
    [HttpGet("include")]
    public async Task<IActionResult> GetStudentsWithEnrollmentCountInclude()
    {
        _logger.LogInformation("=== EXERCISE 7: USING INCLUDE ===");
        
        var students = await _context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
            .ToListAsync();

        var results = students.Select(s => new
        {
            s.RegistrationNumber,
            s.Name,
            s.GPA,
            EnrollmentCount = s.Enrollments.Count
        }).ToList();

        _logger.LogInformation("=== QUERIES: 1 (with Include) ===");
        
        return Ok(new
        {
            Message = "Using Include - 1 query but loads all enrollment data",
            Students = results,
            TotalQueries = 1,
            Note = "One query with JOIN, but loads all Enrollment objects (heavier)"
        });
    }
}