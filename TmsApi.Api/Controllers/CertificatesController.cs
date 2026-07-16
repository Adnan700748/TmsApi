using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/certificates")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CertificatesController(ICertificateService certificateService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CertificateResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List certificates with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS certificates. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCertificates([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await certificateService.GetCertificatesAsync(request, ct);

        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCertificateById))]
    [ProducesResponseType(typeof(CertificateDetailDto),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a certificate by ID")]
    [EndpointDescription("Returns certificate details with HATEOAS links. Returns 404 if the certificate does not exist.")]
    public async Task<IActionResult> GetCertificateById(int id, CancellationToken ct)
    {
        var certificate = await certificateService.GetByIdAsync(id, ct);

        if (certificate is null)
            return NotFound();

        var selfPath = linkGenerator.GetPathByName(HttpContext, nameof(GetCertificateById), new { id })!;

        var links = new List<LinkDto>
        {
            new(selfPath, "self", "GET"),
            new(selfPath, "update", "PUT"),
            new(selfPath, "delete", "DELETE")
        };

        var detail = new CertificateDetailDto
        {
            Id = certificate.Id,
            SerialNumber = certificate.SerialNumber,
            IssuedAt = certificate.IssuedAt,
            StudentId = certificate.StudentId,
            CourseId = certificate.CourseId,
            Links = links
        };

        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new certificate")]
    [EndpointDescription("Creates a certificate with a unique serial number. Returns 409 if the serial number already exists.")]
    public async Task<IActionResult> CreateCertificate(CreateCertificateRequest request, CancellationToken ct)
    {
        if (await certificateService.SerialNumberExistsAsync(request.SerialNumber, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Certificate serial number already exists",
                Detail = $"A certificate with serial number '{request.SerialNumber}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await certificateService.CreateAsync(request, ct);

        return CreatedAtAction(nameof(GetCertificateById), new { id = result.Id }, result);
    }
}