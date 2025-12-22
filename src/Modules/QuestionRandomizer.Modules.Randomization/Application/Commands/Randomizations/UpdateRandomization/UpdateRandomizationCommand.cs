namespace QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.UpdateRandomization;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;

/// <summary>
/// Command to update an existing randomization session
/// </summary>
public record UpdateRandomizationCommand : IRequest<RandomizationDto>
{
    public string Id { get; init; } = string.Empty;
    public bool ShowAnswer { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CurrentQuestionId { get; init; }
}
