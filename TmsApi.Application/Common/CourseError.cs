namespace TmsApi.Application.Common;

public sealed record CourseError(string Code, string Message)
{
    public static CourseError NotFound(int id) =>
        new("course_not_found", $"Course with ID {id} was not found.");

    public static CourseError NotFoundByCode(string code) =>
        new("course_not_found", $"Course with code '{code}' was not found.");

    public static CourseError DuplicateCode(string code) =>
        new("duplicate_code", $"A course with code '{code}' already exists.");

    public static CourseError InvalidCapacity(int capacity) =>
        new("invalid_capacity", $"Max capacity must be greater than 0. Provided: {capacity}");

    public static CourseError HasEnrollments(int id) =>
        new("has_enrollments", $"Course {id} has active enrollments and cannot be deleted.");
}