namespace QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.CreateRandomization;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;

/// <summary>
/// Command to create a new randomization session
/// </summary>
public record CreateRandomizationCommand : IRequest<RandomizationDto>
{
}
