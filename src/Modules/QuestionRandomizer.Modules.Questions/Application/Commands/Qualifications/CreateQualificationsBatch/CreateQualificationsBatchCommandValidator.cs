namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.CreateQualificationsBatch;

using FluentValidation;

/// <summary>
/// Validator for CreateQualificationsBatchCommand
/// </summary>
public class CreateQualificationsBatchCommandValidator : AbstractValidator<CreateQualificationsBatchCommand>
{
    public CreateQualificationsBatchCommandValidator()
    {
        RuleFor(x => x.QualificationNames)
            .NotEmpty().WithMessage("At least one qualification name is required")
            .Must(names => names.Count <= 100).WithMessage("Maximum 100 qualifications can be created at once");

        RuleForEach(x => x.QualificationNames)
            .NotEmpty().WithMessage("Qualification name cannot be empty")
            .MaximumLength(100).WithMessage("Qualification name must not exceed 100 characters");
    }
}
