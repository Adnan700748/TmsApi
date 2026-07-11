using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateStudentRequest
{
    [Required]
    public required string RegistrationNumber { get; init; }

    [Required]
    [StringLength(200)]
    public required string Name { get; init; }

    [Range(0, 4)]
    public decimal GPA { get; init; }

    public bool IsActive { get; init; } = true;
}