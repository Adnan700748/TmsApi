namespace TmsApi.Dtos;

public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string Name,
    decimal GPA,
    bool IsActive);