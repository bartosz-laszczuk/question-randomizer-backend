namespace QuestionRandomizer.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Infrastructure.Authorization;
using QuestionRandomizer.Application.Commands.Randomizations.CreateRandomization;
using QuestionRandomizer.Application.Commands.Randomizations.UpdateRandomization;
using QuestionRandomizer.Application.Commands.Randomizations.ClearCurrentQuestion;
using QuestionRandomizer.Application.Commands.SelectedCategories.AddSelectedCategory;
using QuestionRandomizer.Application.Commands.SelectedCategories.DeleteSelectedCategory;
using QuestionRandomizer.Application.Commands.UsedQuestions.AddUsedQuestion;
using QuestionRandomizer.Application.Commands.UsedQuestions.DeleteUsedQuestion;
using QuestionRandomizer.Application.Commands.UsedQuestions.UpdateUsedQuestionCategory;
using QuestionRandomizer.Application.Commands.PostponedQuestions.AddPostponedQuestion;
using QuestionRandomizer.Application.Commands.PostponedQuestions.DeletePostponedQuestion;
using QuestionRandomizer.Application.Commands.PostponedQuestions.UpdatePostponedQuestionTimestamp;
using QuestionRandomizer.Application.Queries.Randomizations.GetRandomization;
using QuestionRandomizer.Application.Queries.SelectedCategories.GetSelectedCategories;
using QuestionRandomizer.Application.Queries.UsedQuestions.GetUsedQuestions;
using QuestionRandomizer.Application.Queries.PostponedQuestions.GetPostponedQuestions;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Manages randomization sessions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class RandomizationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RandomizationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the active randomization session for the authenticated user
    /// </summary>
    /// <returns>Active randomization session or null</returns>
    [HttpGet]
    [ProducesResponseType(typeof(RandomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRandomization(CancellationToken cancellationToken = default)
    {
        var query = new GetRandomizationQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new randomization session
    /// </summary>
    /// <param name="command">Randomization details</param>
    /// <returns>Created randomization</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RandomizationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRandomization([FromBody] CreateRandomizationCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRandomization), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing randomization session
    /// </summary>
    /// <param name="id">Randomization ID</param>
    /// <param name="command">Updated randomization details</param>
    /// <returns>Updated randomization</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RandomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRandomization(string id, [FromBody] UpdateRandomizationCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Clear the current question from the randomization session
    /// </summary>
    /// <param name="id">Randomization ID</param>
    /// <returns>No content on success</returns>
    [HttpPost("{id}/clear-current-question")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearCurrentQuestion(string id, CancellationToken cancellationToken = default)
    {
        var command = new ClearCurrentQuestionCommand { RandomizationId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // ================ Selected Categories ================

    /// <summary>
    /// Get all selected categories for a randomization session
    /// </summary>
    [HttpGet("{id}/selected-categories")]
    [ProducesResponseType(typeof(List<SelectedCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSelectedCategories(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetSelectedCategoriesQuery { RandomizationId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add a category to the selected categories list
    /// </summary>
    [HttpPost("{id}/selected-categories")]
    [ProducesResponseType(typeof(SelectedCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSelectedCategory(string id, [FromBody] AddSelectedCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return BadRequest("Randomization ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSelectedCategories), new { id }, result);
    }

    /// <summary>
    /// Remove a category from the selected categories list
    /// </summary>
    [HttpDelete("{id}/selected-categories/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSelectedCategory(string id, string categoryId, CancellationToken cancellationToken = default)
    {
        var command = new DeleteSelectedCategoryCommand
        {
            RandomizationId = id,
            CategoryId = categoryId
        };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // ================ Used Questions ================

    /// <summary>
    /// Get all used questions for a randomization session
    /// </summary>
    [HttpGet("{id}/used-questions")]
    [ProducesResponseType(typeof(List<UsedQuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsedQuestions(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetUsedQuestionsQuery { RandomizationId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add a question to the used questions list
    /// </summary>
    [HttpPost("{id}/used-questions")]
    [ProducesResponseType(typeof(UsedQuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddUsedQuestion(string id, [FromBody] AddUsedQuestionCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return BadRequest("Randomization ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetUsedQuestions), new { id }, result);
    }

    /// <summary>
    /// Remove a question from the used questions list
    /// </summary>
    [HttpDelete("{id}/used-questions/{questionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUsedQuestion(string id, string questionId, CancellationToken cancellationToken = default)
    {
        var command = new DeleteUsedQuestionCommand
        {
            RandomizationId = id,
            QuestionId = questionId
        };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update category information for used questions
    /// </summary>
    [HttpPut("{id}/used-questions/category")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUsedQuestionCategory(string id, [FromBody] UpdateUsedQuestionCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return BadRequest("Randomization ID in URL does not match ID in request body");
        }

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // ================ Postponed Questions ================

    /// <summary>
    /// Get all postponed questions for a randomization session
    /// </summary>
    [HttpGet("{id}/postponed-questions")]
    [ProducesResponseType(typeof(List<PostponedQuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPostponedQuestions(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetPostponedQuestionsQuery { RandomizationId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add a question to the postponed questions list
    /// </summary>
    [HttpPost("{id}/postponed-questions")]
    [ProducesResponseType(typeof(PostponedQuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPostponedQuestion(string id, [FromBody] AddPostponedQuestionCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return BadRequest("Randomization ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPostponedQuestions), new { id }, result);
    }

    /// <summary>
    /// Remove a question from the postponed questions list
    /// </summary>
    [HttpDelete("{id}/postponed-questions/{questionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePostponedQuestion(string id, string questionId, CancellationToken cancellationToken = default)
    {
        var command = new DeletePostponedQuestionCommand
        {
            RandomizationId = id,
            QuestionId = questionId
        };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update the timestamp of a postponed question
    /// </summary>
    [HttpPut("{id}/postponed-questions/{questionId}/timestamp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePostponedQuestionTimestamp(string id, string questionId, CancellationToken cancellationToken = default)
    {
        var command = new UpdatePostponedQuestionTimestampCommand
        {
            RandomizationId = id,
            QuestionId = questionId
        };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
