namespace TmsApi.Entities;

public record EnrollmentRecord(
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);