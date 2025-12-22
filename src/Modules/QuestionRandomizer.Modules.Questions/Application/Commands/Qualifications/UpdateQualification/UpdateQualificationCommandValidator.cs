namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.UpdateQualification;

using FluentValidation;

/// <summary>
/// Validator for UpdateQualificationCommand
/// </summary>
public class UpdateQualificationCommandValidator : AbstractValidator<UpdateQualificationCommand>
{
    public UpdateQualificationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Qualification ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Qualification name is required")
            .MaximumLength(100).WithMessage("Qualification name must not exceed 100 characters");
    }
}
