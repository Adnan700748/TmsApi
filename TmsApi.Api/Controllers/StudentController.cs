using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentController( IStudentService studentService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List students with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS students. PageSize is capped at 50.")]
    public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await studentService.GetStudentsAsync(request, ct);

        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetStudent))]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student by ID")]
    [EndpointDescription("Returns student details with HATEOAS links. Returns 404 if the student does not exist.")]
    public async Task<IActionResult> GetStudent(int id, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(id, ct);

        if (student is null)
            return NotFound();

        var selfPath = linkGenerator.GetPathByName(HttpContext, nameof(GetStudent), new { id })!;
        var updatePath = linkGenerator.GetPathByName(HttpContext, nameof(UpdateStudent), new { id })!;
        var deletePath = linkGenerator.GetPathByName(HttpContext, nameof(DeleteStudent), new { id })!;
        var enrollmentsPath = linkGenerator.GetPathByName(HttpContext, nameof(GetStudentEnrollments), new { id })!;

        var links = new List<LinkDto>
        {
            new(selfPath, "self", "GET"),
            new(updatePath, "update", "PUT"),
            new(deletePath, "delete", "DELETE"),
            new(enrollmentsPath, "enrollments", "GET")
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

    [HttpPost(Name = nameof(CreateStudent))]
[ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[EndpointSummary("Create a new student")]
[EndpointDescription("Creates a new student. Returns 409 if a student with the same registration number already exists.")]
public async Task<IActionResult> CreateStudent(int id,CreateStudentRequest request, CancellationToken ct)
{
    var result = await studentService.CreateAsync(request, ct);
    
    // Check if student already existed
    var existingStudent = await studentService.GetByIdAsync(id,ct);
    if (existingStudent is not null)
    {
        // If the student already existed, return Conflict with the existing student
        return Conflict(new ProblemDetails
        {
            Title = "Student Already Exists",
            Detail = $"A student with registration number '{request.RegistrationNumber}' already exists.",
            Status = StatusCodes.Status409Conflict,
            Extensions =
            {
                ["existingStudent"] = existingStudent
            }
        });
    }

    // Return 201 Created with location header
    return CreatedAtAction(nameof(GetStudent), new { id = result.Id }, result);
}

    [HttpPut("{id:int}", Name = nameof(UpdateStudent))]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a student")]
    [EndpointDescription("Updates the supplied student fields. Returns 404 if the student does not exist.")]
    public async Task<IActionResult> UpdateStudent(int id, UpdateStudentRequest request, CancellationToken ct)
    {
        var student = await studentService.UpdateAsync(id, request,  ct);

        if (student is null)
            return NotFound();

        return Ok(student);
    }

    [HttpDelete("{id:int}", Name = nameof(DeleteStudent))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a student")]
    [EndpointDescription("Deletes a student. Returns 404 if the student does not exist.")]
    public async Task<IActionResult> DeleteStudent(int id,CancellationToken ct)
    {
        var deleted = await studentService.DeleteAsync(id , ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:int}/enrollments", Name = nameof(GetStudentEnrollments))]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrolments for a student")]
    [EndpointDescription("Returns all course enrolments for a student. Returns 404 if the student does not exist.")]
    public async Task<IActionResult> GetStudentEnrollments(int id, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(id, ct);

        if (student is null)
            return NotFound();

        var enrollments = await studentService.GetEnrollmentsAsync(id, ct);

        return Ok(enrollments);
    }
}