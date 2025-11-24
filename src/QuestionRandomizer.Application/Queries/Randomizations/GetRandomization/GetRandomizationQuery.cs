namespace QuestionRandomizer.Application.Queries.Randomizations.GetRandomization;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get the active randomization session for the current user
/// </summary>
public record GetRandomizationQuery : IRequest<RandomizationDto?>
{
}
