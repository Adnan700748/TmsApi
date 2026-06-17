using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    // to get all courses
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var course = await courseService.GetByCodeAsync(code);
        return course is not null ? Ok(course) : NotFound();
    }

    // creates courses  
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            Capacity = request.Capacity,
          
        };

        var created = await courseService.CreateAsync(course);

        return CreatedAtAction(
            nameof(GetByCode),
            new { code = created.Code },
            created);
    }

   
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound();
    }

    
}

public record CreateCourseRequest(
    string Code,
    string Title,
    int Capacity);