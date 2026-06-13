using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    // GET /api/students returns all students
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await studentService.GetAllAsync();
        return Ok(students);
    }

    // GET /api/students/{id} returns one or 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await studentService.GetByIdAsync(id);
        return student is not null ? Ok(student) : NotFound();
    }

    // POST /api/students creates and returns 201 with Location header
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var student = new Student
        {
            Id = request.Id,
            Name = request.Name,
            Age = request.Age,
            GPA = request.GPA
        };

        var created = await studentService.CreateAsync(student);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // DELETE /api/students/{id} returns 204 or 404
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateStudentRequest(
    string Id,
    string Name,
    int Age,
    decimal GPA);