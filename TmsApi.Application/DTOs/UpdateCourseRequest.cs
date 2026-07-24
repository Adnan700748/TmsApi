using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record UpdateCourseRequest
{
    [Required]
    [StringLength(200)]
    public required string Title { get; init; }

    [Range(1, 1000)]
    public int MaxCapacity { get; init; }
}