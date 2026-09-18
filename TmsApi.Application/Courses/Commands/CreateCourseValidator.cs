using FluentValidation;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator(ICourseService courseService)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches("^[A-Z]{3}-\\d{3}$")
            .WithMessage("Course code must follow the format XXX-000 (e.g., CSE-101).")
            .MustAsync(async (code, ct) =>
                !await courseService.CodeExistsAsync(code, ct))
            .WithMessage("Course code already exists.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0);
    }
}