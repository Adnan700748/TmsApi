using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record CreateAssessmentRequest
{
    [Required]
    [StringLength(200)]
    public required string Title { get; init; }

    [Range(1, 1000)]
    public decimal MaxScore { get; init; }

    [Range(0.01, 1)]
    public decimal Weight { get; init; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; init; }
}