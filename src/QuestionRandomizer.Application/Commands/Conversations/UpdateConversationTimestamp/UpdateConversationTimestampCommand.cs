namespace QuestionRandomizer.Application.Commands.Conversations.UpdateConversationTimestamp;

using MediatR;

/// <summary>
/// Command to update conversation's updatedAt timestamp
/// </summary>
public record UpdateConversationTimestampCommand : IRequest<Unit>
{
    public string ConversationId { get; init; } = string.Empty;
}
