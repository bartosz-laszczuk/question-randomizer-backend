namespace QuestionRandomizer.Modules.Conversations.Application.Queries.Conversations.GetConversationById;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;

/// <summary>
/// Query to get a conversation by ID
/// </summary>
public record GetConversationByIdQuery : IRequest<ConversationDto>
{
    public string Id { get; init; } = string.Empty;
}
