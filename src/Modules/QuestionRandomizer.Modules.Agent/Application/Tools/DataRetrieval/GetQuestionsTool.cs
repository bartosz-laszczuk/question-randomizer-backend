namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for retrieving interview questions with optional filters
/// </summary>
public class GetQuestionsTool : AgentToolBase<GetQuestionsInput>
{
    private readonly IQuestionRepository _questionRepository;

    public GetQuestionsTool(
        IQuestionRepository questionRepository,
        ILogger<GetQuestionsTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "get_questions";

    public override string Description =>
        @"Retrieves interview questions for the user with optional filters.

Use this tool to:
- View all questions for a user
- Filter questions by category
- Limit the number of results
- Get questions for analysis or display

Returns a list of questions with their details (question text, answer, category, etc.)";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""categoryId"": {
      ""type"": ""string"",
      ""description"": ""Optional: Filter questions by category ID""
    },
    ""limit"": {
      ""type"": ""number"",
      ""description"": ""Optional: Maximum number of questions to return (default: 50, max: 1000)"",
      ""minimum"": 1,
      ""maximum"": 1000
    },
    ""includeInactive"": {
      ""type"": ""boolean"",
      ""description"": ""Optional: Include deleted/inactive questions (default: false)""
    }
  }
}";

    protected override GetQuestionsInput ValidateAndParse(JsonElement input)
    {
        var validator = new GetQuestionsInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        GetQuestionsInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        // CRITICAL: Always filter by userId for security
        List<Questions.Domain.Entities.Question> questions;

        if (!string.IsNullOrEmpty(input.CategoryId))
        {
            // Get questions by category
            questions = await _questionRepository.GetByCategoryIdAsync(
                input.CategoryId,
                userId,
                cancellationToken);
        }
        else
        {
            // Get all questions for user
            questions = await _questionRepository.GetByUserIdAsync(
                userId,
                cancellationToken);
        }

        // Filter out inactive questions unless requested
        if (!input.IncludeInactive)
        {
            questions = questions.Where(q => q.IsActive).ToList();
        }

        // Apply limit
        var limit = input.Limit ?? 50;
        questions = questions.Take(limit).ToList();

        return new
        {
            success = true,
            count = questions.Count,
            questions = questions.Select(q => new
            {
                id = q.Id,
                questionText = q.QuestionText,
                answer = q.Answer,
                answerPl = q.AnswerPl,
                categoryId = q.CategoryId,
                categoryName = q.CategoryName,
                qualificationId = q.QualificationId,
                qualificationName = q.QualificationName,
                isActive = q.IsActive,
                tags = q.Tags,
                createdAt = q.CreatedAt,
                updatedAt = q.UpdatedAt
            })
        };
    }
}

/// <summary>
/// Input parameters for GetQuestionsTool
/// </summary>
public class GetQuestionsInput
{
    public string? CategoryId { get; set; }
    public int? Limit { get; set; }
    public bool IncludeInactive { get; set; } = false;
}

/// <summary>
/// Validator for GetQuestionsInput
/// </summary>
public class GetQuestionsInputValidator : AbstractValidator<GetQuestionsInput>
{
    public GetQuestionsInputValidator()
    {
        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .InclusiveBetween(1, 1000)
                .WithMessage("Limit must be between 1 and 1000");
        });

        When(x => !string.IsNullOrEmpty(x.CategoryId), () =>
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .WithMessage("CategoryId cannot be empty if provided");
        });
    }
}
