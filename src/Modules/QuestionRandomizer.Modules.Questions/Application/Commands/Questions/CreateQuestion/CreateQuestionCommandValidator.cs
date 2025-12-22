namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.CreateQuestion;

using FluentValidation;

/// <summary>
/// Validator for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required")
            .MaximumLength(5000).WithMessage("Answer must not exceed 5000 characters");

        RuleFor(x => x.AnswerPl)
            .NotEmpty().WithMessage("Polish answer is required")
            .MaximumLength(5000).WithMessage("Polish answer must not exceed 5000 characters");

        RuleFor(x => x.CategoryId)
            .MaximumLength(100).WithMessage("Category ID must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CategoryId));

        RuleFor(x => x.QualificationId)
            .MaximumLength(100).WithMessage("Qualification ID must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.QualificationId));

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Maximum 20 tags allowed")
            .When(x => x.Tags != null);
    }
}
