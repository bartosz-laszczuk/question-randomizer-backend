namespace QuestionRandomizer.Application.Queries.Conversations.GetConversationById;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get a conversation by ID
/// </summary>
public record GetConversationByIdQuery : IRequest<ConversationDto>
{
    public string Id { get; init; } = string.Empty;
}
