namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for creating a new category
/// </summary>
public class CreateCategoryTool : AgentToolBase<CreateCategoryInput>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryTool(
        ICategoryRepository categoryRepository,
        ILogger<CreateCategoryTool> logger) : base(logger)
    {
        _categoryRepository = categoryRepository;
    }

    public override string Name => "create_category";

    public override string Description =>
        @"Creates a new question category.

Use this tool to:
- Add new categories for organizing questions
- Create categories needed for categorization tasks

Returns the created category with its ID.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""name"": {
      ""type"": ""string"",
      ""description"": ""The category name (e.g., 'JavaScript', 'Python', 'Algorithms')""
    }
  },
  ""required"": [""name""]
}";

    protected override CreateCategoryInput ValidateAndParse(JsonElement input)
    {
        var validator = new CreateCategoryInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        CreateCategoryInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = input.Name,
            UserId = userId,
            IsActive = true
        };

        var created = await _categoryRepository.CreateAsync(category, cancellationToken);

        return new
        {
            success = true,
            message = "Category created successfully",
            category = new
            {
                id = created.Id,
                name = created.Name
            }
        };
    }
}

public class CreateCategoryInput
{
    public string Name { get; set; } = string.Empty;
}

public class CreateCategoryInputValidator : AbstractValidator<CreateCategoryInput>
{
    public CreateCategoryInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters");
    }
}
