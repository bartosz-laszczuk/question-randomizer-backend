namespace QuestionRandomizer.Modules.Conversations.Application.Commands.Conversations.CreateConversation;

using FluentValidation;

/// <summary>
/// Validator for CreateConversationCommand
/// </summary>
public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Conversation title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
    }
}
