namespace QuestionRandomizer.Modules.Conversations.Application.Commands.Conversations.DeleteConversation;

using MediatR;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Domain.Exceptions;

/// <summary>
/// Handler for DeleteConversationCommand
/// </summary>
public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand, Unit>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteConversationCommandHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var deleted = await _conversationRepository.DeleteAsync(request.Id, userId, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Conversation with ID {request.Id} not found");
        }

        return Unit.Value;
    }
}
