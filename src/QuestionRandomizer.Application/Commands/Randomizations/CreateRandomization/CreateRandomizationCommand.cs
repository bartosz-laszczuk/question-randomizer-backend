namespace QuestionRandomizer.Application.Commands.Randomizations.CreateRandomization;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create a new randomization session
/// </summary>
public record CreateRandomizationCommand : IRequest<RandomizationDto>
{
}
