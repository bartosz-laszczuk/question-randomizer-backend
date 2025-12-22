namespace QuestionRandomizer.Modules.Conversations.Application.Queries.Conversations.GetConversationById;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Domain.Exceptions;

/// <summary>
/// Handler for GetConversationByIdQuery
/// </summary>
public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationByIdQueryHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ConversationDto> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var conversation = await _conversationRepository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (conversation == null)
        {
            throw new NotFoundException($"Conversation with ID {request.Id} not found");
        }

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            IsActive = conversation.IsActive,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }
}
