namespace QuestionRandomizer.Modules.Conversations.Application.Queries.Conversations.GetConversations;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;

/// <summary>
/// Query to get all conversations for the current user
/// </summary>
public record GetConversationsQuery : IRequest<List<ConversationDto>>
{
}
