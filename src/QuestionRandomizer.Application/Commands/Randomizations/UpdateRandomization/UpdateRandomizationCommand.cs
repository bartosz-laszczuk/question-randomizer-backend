namespace QuestionRandomizer.Application.Commands.Randomizations.UpdateRandomization;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to update a randomization session
/// </summary>
public record UpdateRandomizationCommand : IRequest<RandomizationDto>
{
    public string Id { get; init; } = string.Empty;
    public bool ShowAnswer { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CurrentQuestionId { get; init; }
}
