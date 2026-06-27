using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly TmsDbContext _context;
    private readonly ILogger<StudentController> _logger;

    public StudentController(TmsDbContext context, ILogger<StudentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/student - Get all students with audit info
    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive,
                LastUpdated = EF.Property<DateTime>(s, "LastUpdated"),
                Version = s.Version
            })
            .ToListAsync();

        return Ok(students);
    }

    // GET: api/student/{id} - Get single student with audit info
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive,
                LastUpdated = EF.Property<DateTime>(s, "LastUpdated"),
                Version = s.Version
            })
            .FirstOrDefaultAsync();

        if (student == null)
            return NotFound();

        return Ok(student);
    }

    // PUT: api/student/{id}/name - Update student name (triggers LastUpdated)
    [HttpPut("{id}/name")]
    public async Task<IActionResult> UpdateName(int id, [FromBody] UpdateNameRequest request)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return NotFound();

        var oldName = student.Name;
        var oldVersion = student.Version;

        student.Name = request.NewName;

        try
        {
            await _context.SaveChangesAsync();

            var newLastUpdated = _context.Entry(student).Property("LastUpdated").CurrentValue;

            _logger.LogInformation($"Student {student.RegistrationNumber} updated: Name '{oldName}' -> '{request.NewName}'");

            return Ok(new
            {
                student.Id,
                student.RegistrationNumber,
                student.Name,
                student.GPA,
                student.IsActive,
                LastUpdated = newLastUpdated,
                OldVersion = oldVersion,
                NewVersion = student.Version,
                Message = "✅ Name updated successfully! LastUpdated auto-updated."
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning($"Concurrency conflict on Student {student.RegistrationNumber}: {ex.Message}");
            await _context.Entry(student).ReloadAsync();
            
            return Conflict(new
            {
                Message = "❌ Concurrency conflict! Another user modified this student.",
                CurrentName = student.Name,
                CurrentGPA = student.GPA,
                CurrentVersion = student.Version,
                Hint = "Refresh the student data and try again"
            });
        }
    }

    // PUT: api/student/{id}/gpa - Update student GPA (triggers LastUpdated)
    [HttpPut("{id}/gpa")]
    public async Task<IActionResult> UpdateGPA(int id, [FromBody] UpdateGPARequest request)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return NotFound();

        var oldGPA = student.GPA;
        var oldVersion = student.Version;

        student.GPA = request.NewGPA;

        try
        {
            await _context.SaveChangesAsync();

            var newLastUpdated = _context.Entry(student).Property("LastUpdated").CurrentValue;

            _logger.LogInformation($"Student {student.RegistrationNumber} updated: GPA '{oldGPA}' -> '{request.NewGPA}'");

            return Ok(new
            {
                student.Id,
                student.RegistrationNumber,
                student.Name,
                student.GPA,
                student.IsActive,
                LastUpdated = newLastUpdated,
                OldVersion = oldVersion,
                NewVersion = student.Version,
                Message = "✅ GPA updated successfully! LastUpdated auto-updated."
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning($"Concurrency conflict on Student {student.RegistrationNumber}: {ex.Message}");
            await _context.Entry(student).ReloadAsync();
            
            return Conflict(new
            {
                Message = "❌ Concurrency conflict! Another user modified this student.",
                CurrentName = student.Name,
                CurrentGPA = student.GPA,
                CurrentVersion = student.Version,
                Hint = "Refresh the student data and try again"
            });
        }
    }

    // Bulk Archive Enrollments
    [HttpPost("bulk-archive")]
    public async Task<IActionResult> BulkArchiveEnrollments([FromBody] ArchiveRequest request)
    {
        var cutoffDate = request.CutoffDate ?? DateTime.UtcNow.AddYears(-1);
        
        _logger.LogInformation($"Archiving enrollments before {cutoffDate:yyyy-MM-dd}");
        
        // Count before archiving
        var beforeCount = await _context.Enrollments
            .Where(e => e.EnrolledAt < cutoffDate && !e.IsArchived)
            .CountAsync();
        
        if (beforeCount == 0)
        {
            return Ok(new
            {
                Message = "No enrollments to archive",
                ArchivedCount = 0
            });
        }
        
        // BULK UPDATE - single SQL statement
        var archivedCount = await _context.Enrollments
            .Where(e => e.EnrolledAt < cutoffDate && !e.IsArchived)
            .ExecuteUpdateAsync(
                setter => setter
                    .SetProperty(e => e.IsArchived, true)
            );
        
        _logger.LogInformation($"✅ Archived {archivedCount} enrollments in one UPDATE statement");
        
        return Ok(new
        {
            Message = $"✅ Archived {archivedCount} enrollments older than {cutoffDate:yyyy-MM-dd}",
            ArchivedCount = archivedCount,
            CutoffDate = cutoffDate,
            Note = "Single SQL UPDATE statement used - no row-by-row processing"
        });
    }

    // Soft Delete Student
    [HttpPost("soft-delete/{id}")]
    public async Task<IActionResult> SoftDeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return NotFound();
        
        if (!student.IsActive)
        {
            return BadRequest(new
            {
                Message = "Student is already soft-deleted",
                student.Id,
                student.Name
            });
        }
        
        student.IsActive = false;
        await _context.SaveChangesAsync();
        
        var lastUpdated = _context.Entry(student).Property("LastUpdated").CurrentValue;
        
        _logger.LogInformation($"Soft-deleted student: {student.RegistrationNumber} - {student.Name}");
        
        return Ok(new
        {
            Message = "✅ Student soft-deleted successfully",
            student.Id,
            student.RegistrationNumber,
            student.Name,
            student.GPA,
            IsActive = student.IsActive,
            LastUpdated = lastUpdated,
            Note = "Student will be hidden from normal queries due to HasQueryFilter"
        });
    }

    // 🔥 EXERCISE 9: Restore Soft-Deleted Student
    [HttpPost("restore/{id}")]
    public async Task<IActionResult> RestoreStudent(int id)
    {
        // Use IgnoreQueryFilters to find soft-deleted students
        var student = await _context.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (student == null)
            return NotFound();
        
        if (student.IsActive)
        {
            return BadRequest(new
            {
                Message = "Student is already active",
                student.Id,
                student.Name
            });
        }
        
        student.IsActive = true;
        await _context.SaveChangesAsync();
        
        var lastUpdated = _context.Entry(student).Property("LastUpdated").CurrentValue;
        
        _logger.LogInformation($"Restored student: {student.RegistrationNumber} - {student.Name}");
        
        return Ok(new
        {
            Message = "✅ Student restored successfully",
            student.Id,
            student.RegistrationNumber,
            student.Name,
            student.GPA,
            IsActive = student.IsActive,
            LastUpdated = lastUpdated,
            Note = "Student is now visible in normal queries again"
        });
    }

    // 🔥 EXERCISE 9: Get Deleted Students (Admin only)
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedStudents()
    {
        var deletedStudents = await _context.Students
            .IgnoreQueryFilters()
            .Where(s => !s.IsActive)
            .Select(s => new
            {
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive,
                LastUpdated = EF.Property<DateTime>(s, "LastUpdated"),
                Version = s.Version
            })
            .ToListAsync();
        
        return Ok(new
        {
            Count = deletedStudents.Count,
            Students = deletedStudents,
            Note = "Using IgnoreQueryFilters() to show soft-deleted students"
        });
    }

    // Request DTOs
    public class UpdateNameRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    public class UpdateGPARequest
    {
        public decimal NewGPA { get; set; }
    }

    public class ArchiveRequest
    {
        public DateTime? CutoffDate { get; set; }
    }
}