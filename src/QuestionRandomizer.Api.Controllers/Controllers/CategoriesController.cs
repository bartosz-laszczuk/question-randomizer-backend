namespace QuestionRandomizer.Api.Controllers.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Application.Commands.Categories.CreateCategory;
using QuestionRandomizer.Application.Commands.Categories.CreateCategoriesBatch;
using QuestionRandomizer.Application.Commands.Categories.UpdateCategory;
using QuestionRandomizer.Application.Commands.Categories.DeleteCategory;
using QuestionRandomizer.Application.Queries.Categories.GetCategories;
using QuestionRandomizer.Application.Queries.Categories.GetCategoryById;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Manages question categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all categories for the authenticated user
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="command">Category details</param>
    /// <returns>Created category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Create multiple categories at once
    /// </summary>
    /// <param name="command">List of category names</param>
    /// <returns>Created categories</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategoriesBatch(
        [FromBody] CreateCategoriesBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategories), result);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="command">Updated category details</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        string id,
        [FromBody] UpdateCategoryCommand command,
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
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(string id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
