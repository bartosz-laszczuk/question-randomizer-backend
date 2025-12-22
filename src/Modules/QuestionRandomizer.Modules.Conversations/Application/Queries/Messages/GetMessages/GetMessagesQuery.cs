namespace QuestionRandomizer.Modules.Conversations.Application.Queries.Messages.GetMessages;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;

/// <summary>
/// Query to get all messages in a conversation
/// </summary>
public record GetMessagesQuery : IRequest<List<MessageDto>>
{
    public string ConversationId { get; init; } = string.Empty;
}
