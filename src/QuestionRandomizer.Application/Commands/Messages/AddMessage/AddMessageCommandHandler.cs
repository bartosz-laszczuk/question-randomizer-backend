namespace QuestionRandomizer.Application.Commands.Messages.AddMessage;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for AddMessageCommand
/// </summary>
public class AddMessageCommandHandler : IRequestHandler<AddMessageCommand, MessageDto>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<MessageDto> Handle(AddMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Verify conversation exists and belongs to user
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, userId, cancellationToken);
        if (conversation == null)
        {
            throw new NotFoundException($"Conversation with ID {request.ConversationId} not found");
        }

        var message = new Message
        {
            ConversationId = request.ConversationId,
            Role = request.Role,
            Content = request.Content,
            Timestamp = DateTime.UtcNow
        };

        var created = await _messageRepository.CreateAsync(message, cancellationToken);

        return new MessageDto
        {
            Id = created.Id,
            ConversationId = created.ConversationId,
            Role = created.Role,
            Content = created.Content,
            Timestamp = created.Timestamp
        };
    }
}
