namespace QuestionRandomizer.Application.Commands.Conversations.CreateConversation;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create a new conversation
/// </summary>
public record CreateConversationCommand : IRequest<ConversationDto>
{
    public string Title { get; init; } = string.Empty;
}
