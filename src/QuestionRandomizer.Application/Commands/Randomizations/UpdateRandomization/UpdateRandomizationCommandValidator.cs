namespace QuestionRandomizer.Application.Commands.Randomizations.UpdateRandomization;

using FluentValidation;

/// <summary>
/// Validator for UpdateRandomizationCommand
/// </summary>
public class UpdateRandomizationCommandValidator : AbstractValidator<UpdateRandomizationCommand>
{
    public UpdateRandomizationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Randomization ID is required");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => status == "Ongoing" || status == "Completed")
            .WithMessage("Status must be 'Ongoing' or 'Completed'");
    }
}
