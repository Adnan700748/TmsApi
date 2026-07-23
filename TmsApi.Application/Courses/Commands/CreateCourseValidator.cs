using FluentValidation;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Course code is required.");

        RuleFor(x => x.Code)
            .Matches(@"^[A-Z]{3}-\d{3}$")
            .WithMessage("Course code must follow the format XXX-000 (e.g., CSE-101).");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Course title is required.")
            .MaximumLength(200)
            .WithMessage("Course title cannot exceed 200 characters.");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0)
            .WithMessage("Max capacity must be greater than 0.");
    }
}