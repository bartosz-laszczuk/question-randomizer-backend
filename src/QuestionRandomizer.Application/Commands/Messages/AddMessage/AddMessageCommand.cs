namespace QuestionRandomizer.Application.Commands.Messages.AddMessage;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to add a message to a conversation
/// </summary>
public record AddMessageCommand : IRequest<MessageDto>
{
    public string ConversationId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty; // "user" or "assistant"
    public string Content { get; init; } = string.Empty;
}
