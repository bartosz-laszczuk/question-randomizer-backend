namespace QuestionRandomizer.Application.Commands.Conversations.UpdateConversationTimestamp;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for UpdateConversationTimestampCommand
/// </summary>
public class UpdateConversationTimestampCommandHandler : IRequestHandler<UpdateConversationTimestampCommand, Unit>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateConversationTimestampCommandHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateConversationTimestampCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var updated = await _conversationRepository.UpdateTimestampAsync(request.ConversationId, userId, cancellationToken);
        if (!updated)
        {
            throw new NotFoundException($"Conversation with ID {request.ConversationId} not found");
        }

        return Unit.Value;
    }
}
