namespace QuestionRandomizer.Modules.Conversations.Application.Queries.Messages.GetMessages;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Domain.Exceptions;

/// <summary>
/// Handler for GetMessagesQuery
/// </summary>
public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, List<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMessagesQueryHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Verify conversation exists and belongs to user
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, userId, cancellationToken);
        if (conversation == null)
        {
            throw new NotFoundException($"Conversation with ID {request.ConversationId} not found");
        }

        var messages = await _messageRepository.GetByConversationIdAsync(request.ConversationId, cancellationToken);

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            Role = m.Role,
            Content = m.Content,
            Timestamp = m.Timestamp
        }).ToList();
    }
}
