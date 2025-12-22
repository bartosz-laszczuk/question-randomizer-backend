namespace QuestionRandomizer.SharedKernel.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// </summary>
    public NotFoundException() : base("The requested entity was not found")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with entity details
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found</param>
    /// <param name="entityId">The identifier of the entity that was not found</param>
    public NotFoundException(string entityName, string entityId)
        : base($"{entityName} with ID '{entityId}' was not found")
    {
    }
}
