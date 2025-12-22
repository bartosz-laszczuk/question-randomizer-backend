using MediatR;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Questions.Domain.Events;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

namespace QuestionRandomizer.Modules.Questions.Application.EventHandlers;

public class CategoryDeletedEventHandler : INotificationHandler<CategoryDeletedEvent>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<CategoryDeletedEventHandler> _logger;

    public CategoryDeletedEventHandler(
        IQuestionRepository questionRepository,
        ILogger<CategoryDeletedEventHandler> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    public async Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CategoryDeletedEvent. Cleaning up category references. CategoryId: {CategoryId}",
            notification.CategoryId);

        await _questionRepository.RemoveCategoryIdAsync(
            notification.CategoryId,
            notification.UserId,
            cancellationToken);

        _logger.LogInformation(
            "Completed cleanup of category references. CategoryId: {CategoryId}",
            notification.CategoryId);
    }
}
