namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.DeleteCategory;

using MediatR;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Domain.Exceptions;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Events;

/// <summary>
/// Handler for DeleteCategoryCommand
/// </summary>
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var deleted = await _categoryRepository.DeleteAsync(request.Id, userId, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found");
        }

        // Publish domain event to clean up category references in questions
        await _mediator.Publish(new CategoryDeletedEvent(request.Id, userId), cancellationToken);

        return Unit.Value;
    }
}
