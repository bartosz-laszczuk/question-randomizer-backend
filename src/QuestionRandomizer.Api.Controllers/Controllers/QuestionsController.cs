namespace QuestionRandomizer.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.UpdateQuestion;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.DeleteQuestion;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.CreateQuestionsBatch;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.UpdateQuestionsBatch;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.RemoveCategoryFromQuestions;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.RemoveQualificationFromQuestions;
using QuestionRandomizer.Modules.Questions.Application.Queries.Questions.GetQuestions;
using QuestionRandomizer.Modules.Questions.Application.Queries.Questions.GetQuestionById;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Manages interview questions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class QuestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all questions for the authenticated user
    /// </summary>
    /// <param name="categoryId">Optional: filter by category</param>
    /// <param name="isActive">Optional: filter by active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of questions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery
        {
            CategoryId = categoryId,
            IsActive = isActive
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific question by ID
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Question details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new question
    /// </summary>
    /// <param name="command">Question details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created question</returns>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuestion(
        [FromBody] CreateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuestionById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing question
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="command">Updated question details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated question</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuestion(
        string id,
        [FromBody] UpdateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a question (soft delete)
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteQuestionCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Create multiple questions in a batch
    /// </summary>
    /// <param name="command">List of questions to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created questions</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(List<QuestionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuestionsBatch(
        [FromBody] CreateQuestionsBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuestions), result);
    }

    /// <summary>
    /// Update multiple questions in a batch
    /// </summary>
    /// <param name="command">List of questions to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("batch")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuestionsBatch(
        [FromBody] UpdateQuestionsBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Remove category from all questions that reference it
    /// </summary>
    /// <param name="categoryId">Category ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("category/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveCategoryFromQuestions(
        string categoryId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveCategoryFromQuestionsCommand { CategoryId = categoryId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Remove qualification from all questions that reference it
    /// </summary>
    /// <param name="qualificationId">Qualification ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("qualification/{qualificationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveQualificationFromQuestions(
        string qualificationId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveQualificationFromQuestionsCommand { QualificationId = qualificationId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
