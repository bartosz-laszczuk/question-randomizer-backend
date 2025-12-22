namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.UpdateQuestionsBatch;

using FluentValidation;

/// <summary>
/// Validator for UpdateQuestionsBatchCommand
/// </summary>
public class UpdateQuestionsBatchCommandValidator : AbstractValidator<UpdateQuestionsBatchCommand>
{
    public UpdateQuestionsBatchCommandValidator()
    {
        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("At least one question is required")
            .Must(questions => questions.Count <= 100).WithMessage("Maximum 100 questions can be updated at once");

        RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.Id)
                .NotEmpty().WithMessage("Question ID is required");

            question.RuleFor(q => q.QuestionText)
                .NotEmpty().WithMessage("Question text is required")
                .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters");

            question.RuleFor(q => q.Answer)
                .NotEmpty().WithMessage("Answer is required")
                .MaximumLength(5000).WithMessage("Answer must not exceed 5000 characters");

            question.RuleFor(q => q.AnswerPl)
                .NotEmpty().WithMessage("Polish answer is required")
                .MaximumLength(5000).WithMessage("Polish answer must not exceed 5000 characters");
        });
    }
}
