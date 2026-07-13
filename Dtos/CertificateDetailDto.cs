namespace TmsApi.Dtos;

public record CertificateDetailDto
{
    public required int Id { get; init; }
    public required string SerialNumber { get; init; }
    public required DateTime IssuedAt { get; init; }
    public required int StudentId { get; init; }
    public required int CourseId { get; init; }
    public required IReadOnlyList<LinkDto> Links { get; init; }
}