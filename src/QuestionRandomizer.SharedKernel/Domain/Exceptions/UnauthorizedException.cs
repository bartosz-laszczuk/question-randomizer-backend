namespace QuestionRandomizer.SharedKernel.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts an unauthorized action
/// </summary>
public class UnauthorizedException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class
    /// </summary>
    public UnauthorizedException() : base("Unauthorized access")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the authorization error</param>
    public UnauthorizedException(string message) : base(message)
    {
    }
}
