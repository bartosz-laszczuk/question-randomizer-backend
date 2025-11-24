namespace QuestionRandomizer.Application.Queries.Conversations.GetConversations;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all conversations for the current user
/// </summary>
public record GetConversationsQuery : IRequest<List<ConversationDto>>
{
}
