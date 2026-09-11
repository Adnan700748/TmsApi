using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record UpdateStudentRequest
{
    [StringLength(200)]
    public string? Name { get; init; }

    [Range(0, 4)]
    public decimal? GPA { get; init; }

    public bool? IsActive { get; init; }
}