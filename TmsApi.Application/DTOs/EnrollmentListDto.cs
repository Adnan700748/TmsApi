namespace TmsApi.Application.DTOs;

public record EnrollmentListDto(
    string Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string Status,
    DateTime EnrolledAt
);