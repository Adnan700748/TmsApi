using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class StudentController( IStudentService studentService, LinkGenerator linkGenerator) : ControllerBase
{
   [HttpGet]
[ProducesResponseType(typeof(PagedResponse<StudentResponseDto>),StatusCodes.Status200OK)]
[EndpointSummary("List students with pagination")]
[EndpointDescription("Returns a paginated, optionally filtered list of TMS students. PageSize is capped at 50.")]
public async Task<IActionResult> GetStudents( [FromQuery] PagedRequest request, CancellationToken ct)
{
    var result = await studentService.GetStudentsAsync(request, ct);
    return Ok(result);
}
   [HttpGet("{id:int}", Name = nameof(GetStudent))]
[ProducesResponseType(typeof(StudentDetailDto),StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
[EndpointSummary("Get a student by ID")]
[EndpointDescription("Returns student details with HATEOAS links. Returns 404 if the student does not exist.")]
public async Task<IActionResult> GetStudent(int id, CancellationToken ct)
{
    var student = await studentService.GetByIdAsync(id, ct);

    if (student is null)
        return NotFound();

    var selfPath = linkGenerator.GetPathByName(
        HttpContext,
        nameof(GetStudent),
        new { id })!;

    var links = new List<LinkDto>
    {
        new(selfPath, "self", "GET"),
        new(selfPath, "update", "PUT"),
        new(selfPath, "delete", "DELETE")
    };

    var detail = new StudentDetailDto
    {
        Id = student.Id,
        RegistrationNumber = student.RegistrationNumber,
        Name = student.Name,
        GPA = student.GPA,
        IsActive = student.IsActive,
        Links = links
    };

    return Ok(detail);
}

[HttpPut("{id:int}", Name = nameof(UpdateStudent))]
[ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("Update a student")]
[EndpointDescription("Updates the supplied student fields. Returns 404 if the student does not exist.")]
public async Task<IActionResult> UpdateStudent(
    int id,
    UpdateStudentRequest request)
{
    var student = await studentService.UpdateAsync(id, request);

    if (student is null)
        return NotFound();

    return Ok(student);
}

[HttpDelete("{id:int}", Name = nameof(DeleteStudent))]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("Delete a student")]
[EndpointDescription("Deletes a student. Returns 404 if the student does not exist.")]
public async Task<IActionResult> DeleteStudent(int id)
{
    var deleted = await studentService.DeleteAsync(id.ToString());

    if (!deleted)
        return NotFound();

    return NoContent();
}
    
}