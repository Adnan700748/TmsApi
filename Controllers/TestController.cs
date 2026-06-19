using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    // Non-translatable helper method
    private static bool IsHonorRoll(decimal gpa) => gpa >= 3.5m;

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA)) // EF Core does not know how to map this method to SQL
                .ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("translation-fix-server")]
    public IActionResult TestTranslationFixServer()
    {
        // Resolution 1: Server-side evaluation (preferred) — inline logic EF Core can translate
        var students = context.Students
            .Where(s => s.GPA >= 3.5m)
            .ToList();
        return Ok(students);
    }

    [HttpGet("translation-fix-client")]
    public IActionResult TestTranslationFixClient()
    {
        // Resolution 2: Client-side evaluation — pulls ALL rows into memory first (watch the SQL log)
        var students = context.Students
            .AsEnumerable()                     // Pulls all rows into application RAM
            .Where(s => IsHonorRoll(s.GPA))
            .ToList();
        return Ok(students);
    }

    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); // Execution is triggered here

        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");

        return Ok(results);
    }

}