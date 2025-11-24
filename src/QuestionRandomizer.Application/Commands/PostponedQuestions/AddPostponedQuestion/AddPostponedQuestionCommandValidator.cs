namespace QuestionRandomizer.Application.Commands.PostponedQuestions.AddPostponedQuestion;

using FluentValidation;

/// <summary>
/// Validator for AddPostponedQuestionCommand
/// </summary>
public class AddPostponedQuestionCommandValidator : AbstractValidator<AddPostponedQuestionCommand>
{
    public AddPostponedQuestionCommandValidator()
    {
        RuleFor(x => x.RandomizationId)
            .NotEmpty().WithMessage("Randomization ID is required");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");
    }
}
