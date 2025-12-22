namespace QuestionRandomizer.Modules.Conversations.Application.Queries.Conversations.GetConversations;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for GetConversationsQuery
/// </summary>
public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationsQueryHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var conversations = await _conversationRepository.GetByUserIdAsync(userId, cancellationToken);

        return conversations.Select(c => new ConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}
