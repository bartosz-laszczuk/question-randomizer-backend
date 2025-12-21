namespace QuestionRandomizer.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Infrastructure.Authorization;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualification;
using QuestionRandomizer.Application.Commands.Qualifications.UpdateQualification;
using QuestionRandomizer.Application.Commands.Qualifications.DeleteQualification;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualificationsBatch;
using QuestionRandomizer.Application.Queries.Qualifications.GetQualifications;
using QuestionRandomizer.Application.Queries.Qualifications.GetQualificationById;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Manages job qualifications
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class QualificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QualificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all qualifications for the authenticated user
    /// </summary>
    /// <returns>List of qualifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<QualificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQualifications(CancellationToken cancellationToken = default)
    {
        var query = new GetQualificationsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific qualification by ID
    /// </summary>
    /// <param name="id">Qualification ID</param>
    /// <returns>Qualification details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QualificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQualificationById(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetQualificationByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new qualification
    /// </summary>
    /// <param name="command">Qualification details</param>
    /// <returns>Created qualification</returns>
    [HttpPost]
    [ProducesResponseType(typeof(QualificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQualification([FromBody] CreateQualificationCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQualificationById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Create multiple qualifications from a list of names
    /// </summary>
    /// <param name="command">List of qualification names</param>
    /// <returns>Created qualifications</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(List<QualificationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQualificationsBatch([FromBody] CreateQualificationsBatchCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQualifications), result);
    }

    /// <summary>
    /// Update an existing qualification
    /// </summary>
    /// <param name="id">Qualification ID</param>
    /// <param name="command">Updated qualification details</param>
    /// <returns>Updated qualification</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(QualificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQualification(string id, [FromBody] UpdateQualificationCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a qualification (soft delete)
    /// </summary>
    /// <param name="id">Qualification ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQualification(string id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteQualificationCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
