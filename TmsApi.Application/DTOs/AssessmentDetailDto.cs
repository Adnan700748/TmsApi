namespace TmsApi.Application.DTOs;

public record AssessmentDetailDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required decimal MaxScore { get; init; }
    public required decimal Weight { get; init; }
    public required int CourseId { get; init; }
    public required IReadOnlyList<LinkDto> Links { get; init; }
}