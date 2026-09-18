using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record CreateCertificateRequest
{
    [Required]
    public required string SerialNumber { get; init; }

    [Range(1, int.MaxValue)]
    public required int StudentId { get; init; }

    [Range(1, int.MaxValue)]
    public required int CourseId { get; init; }
}