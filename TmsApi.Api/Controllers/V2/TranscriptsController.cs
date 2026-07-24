using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public IActionResult RequestTranscript([FromBody] object? _)
    {
        // Stub for Session 2.
        // Exercise 5 will replace this with:
        // - enqueue background job
        // - HTTP 202 Accepted
        // - Location header
        return Ok(new
        {
            message = "Transcript request accepted (stub)."
        });
    }
}