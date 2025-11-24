namespace QuestionRandomizer.Application.Commands.Conversations.DeleteConversation;

using MediatR;

/// <summary>
/// Command to delete a conversation and all its messages
/// </summary>
public record DeleteConversationCommand : IRequest<Unit>
{
    public string Id { get; init; } = string.Empty;
}
