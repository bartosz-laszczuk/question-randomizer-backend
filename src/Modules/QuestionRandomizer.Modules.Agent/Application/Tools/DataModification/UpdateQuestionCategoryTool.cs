namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for updating a question's category assignment
/// </summary>
public class UpdateQuestionCategoryTool : AgentToolBase<UpdateQuestionCategoryInput>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public UpdateQuestionCategoryTool(
        IQuestionRepository questionRepository,
        ICategoryRepository categoryRepository,
        ILogger<UpdateQuestionCategoryTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
        _categoryRepository = categoryRepository;
    }

    public override string Name => "update_question_category";

    public override string Description =>
        @"Updates the category assignment for a question.

Use this tool to:
- Assign a category to a question
- Change a question's category
- Remove category from a question (by setting categoryId to null)

This is the primary tool for categorizing questions.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""questionId"": {
      ""type"": ""string"",
      ""description"": ""The ID of the question to update""
    },
    ""categoryId"": {
      ""type"": ""string"",
      ""description"": ""The category ID to assign (or null to remove category)""
    }
  },
  ""required"": [""questionId""]
}";

    protected override UpdateQuestionCategoryInput ValidateAndParse(JsonElement input)
    {
        var validator = new UpdateQuestionCategoryInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        UpdateQuestionCategoryInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        // CRITICAL: Always verify userId matches the question owner
        var question = await _questionRepository.GetByIdAsync(
            input.QuestionId,
            userId,
            cancellationToken);

        if (question == null)
        {
            return new
            {
                success = false,
                error = $"Question with ID '{input.QuestionId}' not found or unauthorized"
            };
        }

        // Resolve category name if categoryId is provided
        string? categoryName = null;
        if (!string.IsNullOrEmpty(input.CategoryId))
        {
            var category = await _categoryRepository.GetByIdAsync(
                input.CategoryId,
                userId,
                cancellationToken);

            if (category == null)
            {
                return new
                {
                    success = false,
                    error = $"Category with ID '{input.CategoryId}' not found or unauthorized"
                };
            }

            categoryName = category.Name;
        }

        // Update the question
        question.CategoryId = input.CategoryId;
        question.CategoryName = categoryName;
        question.UpdatedAt = DateTime.UtcNow;

        var updated = await _questionRepository.UpdateAsync(question, cancellationToken);

        if (!updated)
        {
            return new
            {
                success = false,
                error = "Failed to update question"
            };
        }

        return new
        {
            success = true,
            message = "Question category updated successfully",
            question = new
            {
                id = question.Id,
                questionText = question.QuestionText,
                categoryId = question.CategoryId,
                categoryName = question.CategoryName,
                updatedAt = question.UpdatedAt
            }
        };
    }
}

/// <summary>
/// Input parameters for UpdateQuestionCategoryTool
/// </summary>
public class UpdateQuestionCategoryInput
{
    public string QuestionId { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
}

/// <summary>
/// Validator for UpdateQuestionCategoryInput
/// </summary>
public class UpdateQuestionCategoryInputValidator : AbstractValidator<UpdateQuestionCategoryInput>
{
    public UpdateQuestionCategoryInputValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("Question ID is required");
    }
}
