using QuestionRandomizer.SharedKernel.Domain.Events;

namespace QuestionRandomizer.Modules.Questions.Domain.Events;

public record QualificationDeletedEvent(string QualificationId, string UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
