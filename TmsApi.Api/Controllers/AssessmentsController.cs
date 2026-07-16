using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/assessments")]
[Tags("Assessments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AssessmentsController( IAssessmentService assessmentService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType( typeof(PagedResponse<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List assessments with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS assessments. PageSize is capped at 50.")]
    public async Task<IActionResult> GetAssessments([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await assessmentService.GetAssessmentsAsync(request, ct);

        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetAssessmentById))]
    [ProducesResponseType(typeof(AssessmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get an assessment by ID")]
    [EndpointDescription("Returns assessment details with HATEOAS links. Returns 404 if the assessment does not exist.")]
    public async Task<IActionResult> GetAssessmentById( int id, CancellationToken ct)
    {
        var assessment = await assessmentService.GetByIdAsync(id, ct);

        if (assessment is null)
            return NotFound();

        var selfPath = linkGenerator.GetPathByName( HttpContext, nameof(GetAssessmentById), new { id })!;

        var links = new List<LinkDto>
        {
            new(selfPath, "self", "GET"),
            new(selfPath, "update", "PUT"),
            new(selfPath, "delete", "DELETE")
        };

        var detail = new AssessmentDetailDto
        {
            Id = assessment.Id,
            Title = assessment.Title,
            MaxScore = assessment.MaxScore,
            Weight = assessment.Weight,
            CourseId = assessment.CourseId,
            Links = links
        };

        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType( typeof(AssessmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails),StatusCodes.Status400BadRequest)]
    [EndpointSummary("Create a new assessment")]
    [EndpointDescription("Creates a new assessment for a course.")]
    public async Task<IActionResult> CreateAssessment(CreateAssessmentRequest request, CancellationToken ct)
    {
        var result = await assessmentService.CreateAsync(request, ct);

        return CreatedAtAction(nameof(GetAssessmentById), new { id = result.Id }, result);
    }
}