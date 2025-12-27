namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for retrieving all categories for a user
/// </summary>
public class GetCategoriesTool : AgentToolBase<GetCategoriesInput>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesTool(
        ICategoryRepository categoryRepository,
        ILogger<GetCategoriesTool> logger) : base(logger)
    {
        _categoryRepository = categoryRepository;
    }

    public override string Name => "get_categories";

    public override string Description =>
        @"Retrieves all question categories for the user.

Use this tool to:
- View all available categories
- Get category IDs and names for categorization
- Check what categories exist before creating new ones

Returns a list of categories with their IDs and names.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {}
}";

    protected override GetCategoriesInput ValidateAndParse(JsonElement input)
    {
        var validator = new GetCategoriesInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        GetCategoriesInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        // CRITICAL: Always filter by userId for security
        var categories = await _categoryRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        // Filter out inactive categories
        categories = categories.Where(c => c.IsActive).ToList();

        return new
        {
            success = true,
            count = categories.Count,
            categories = categories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                isActive = c.IsActive
            })
        };
    }
}

/// <summary>
/// Input parameters for GetCategoriesTool (none required)
/// </summary>
public class GetCategoriesInput
{
    // No parameters needed - get all categories for the user
}

/// <summary>
/// Validator for GetCategoriesInput
/// </summary>
public class GetCategoriesInputValidator : AbstractValidator<GetCategoriesInput>
{
    public GetCategoriesInputValidator()
    {
        // No validation rules - no input parameters
    }
}
