namespace QuestionRandomizer.Application.Commands.Messages.AddMessage;

using FluentValidation;

/// <summary>
/// Validator for AddMessageCommand
/// </summary>
public class AddMessageCommandValidator : AbstractValidator<AddMessageCommand>
{
    public AddMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("Conversation ID is required");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => role == "user" || role == "assistant")
            .WithMessage("Role must be 'user' or 'assistant'");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(10000).WithMessage("Content must not exceed 10000 characters");
    }
}
