using MediatR;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Questions.Domain.Events;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

namespace QuestionRandomizer.Modules.Questions.Application.EventHandlers;

public class QualificationDeletedEventHandler : INotificationHandler<QualificationDeletedEvent>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<QualificationDeletedEventHandler> _logger;

    public QualificationDeletedEventHandler(
        IQuestionRepository questionRepository,
        ILogger<QualificationDeletedEventHandler> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    public async Task Handle(QualificationDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling QualificationDeletedEvent. Cleaning up qualification references. QualificationId: {QualificationId}",
            notification.QualificationId);

        await _questionRepository.RemoveQualificationIdAsync(
            notification.QualificationId,
            notification.UserId,
            cancellationToken);

        _logger.LogInformation(
            "Completed cleanup of qualification references. QualificationId: {QualificationId}",
            notification.QualificationId);
    }
}
