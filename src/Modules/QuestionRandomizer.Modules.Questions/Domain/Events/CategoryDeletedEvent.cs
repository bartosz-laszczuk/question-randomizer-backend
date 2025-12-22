using QuestionRandomizer.SharedKernel.Domain.Events;

namespace QuestionRandomizer.Modules.Questions.Domain.Events;

public record CategoryDeletedEvent(string CategoryId, string UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
