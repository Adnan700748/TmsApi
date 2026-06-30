namespace TmsApi.Entities;

public readonly record struct GradeRecord
{
    public string CourseCode { get; }
    public decimal Score { get; }
    public DateTime GradedAt { get; }

    public GradeRecord(string courseCode, decimal score, DateTime gradedAt)
    {
        if (score < 0 || score > 100)
            throw new ArgumentOutOfRangeException(nameof(score),
                "Score must be between 0 and 100.");

        CourseCode = courseCode;
        Score = score;
        GradedAt = gradedAt;
    }
}