namespace QuestionRandomizer.Application.Commands.UsedQuestions.AddUsedQuestion;

using FluentValidation;

/// <summary>
/// Validator for AddUsedQuestionCommand
/// </summary>
public class AddUsedQuestionCommandValidator : AbstractValidator<AddUsedQuestionCommand>
{
    public AddUsedQuestionCommandValidator()
    {
        RuleFor(x => x.RandomizationId)
            .NotEmpty().WithMessage("Randomization ID is required");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");
    }
}
