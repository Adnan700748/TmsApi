using Microsoft.AspNetCore.Mvc;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine(
            "\n>>> STEP 1: Building the query object (no database contact)...");

        var query = context.Students
            .Where(s => s.GPA >= 3.0m);

        Console.WriteLine(
            ">>> STEP 2: Appending a sorting clause...");

        var orderedQuery = query
            .OrderBy(s => s.Name);

        Console.WriteLine(
            ">>> STEP 3: Materializing query into a C# List...");

        var results = orderedQuery.ToList();

        Console.WriteLine(
            ">>> STEP 4: Materialization finished.");

        return Ok(results);
    }
}