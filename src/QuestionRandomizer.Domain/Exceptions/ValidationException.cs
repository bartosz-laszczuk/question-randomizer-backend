namespace QuestionRandomizer.Domain.Exceptions;

/// <summary>
/// Exception thrown when domain validation fails
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// </summary>
    public ValidationException() : base("Validation failed")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the validation error</param>
    public ValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with validation errors
    /// </summary>
    /// <param name="errors">Dictionary of validation errors by field name</param>
    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred")
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets the validation errors
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; }
}
