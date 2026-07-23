using FluentValidation;

namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Course ID must be a positive number.");

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