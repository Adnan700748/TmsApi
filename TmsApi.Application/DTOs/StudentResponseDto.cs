namespace TmsApi.Application.DTOs;

public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string Name,
    decimal GPA,
    bool IsActive);