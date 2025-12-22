namespace QuestionRandomizer.Modules.Conversations.Application.Commands.Conversations.CreateConversation;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;
using QuestionRandomizer.Modules.Conversations.Domain.Entities;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for CreateConversationCommand
/// </summary>
public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var conversation = new Conversation
        {
            UserId = userId,
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _conversationRepository.CreateAsync(conversation, cancellationToken);

        return new ConversationDto
        {
            Id = created.Id,
            Title = created.Title,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
    }
}
