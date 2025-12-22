namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.CreateQualification;

using FluentValidation;

/// <summary>
/// Validator for CreateQualificationCommand
/// </summary>
public class CreateQualificationCommandValidator : AbstractValidator<CreateQualificationCommand>
{
    public CreateQualificationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Qualification name is required")
            .MaximumLength(100).WithMessage("Qualification name must not exceed 100 characters");
    }
}
