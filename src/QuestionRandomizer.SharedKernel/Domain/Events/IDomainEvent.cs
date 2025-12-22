using MediatR;

namespace QuestionRandomizer.SharedKernel.Domain.Events;

/// <summary>
/// Base interface for all domain events
/// Domain events are published via MediatR and handled by event handlers across modules
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// User ID associated with the event (for authorization)
    /// </summary>
    string UserId { get; }
}
