namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for retrieving questions that don't have a category assigned
/// </summary>
public class GetUncategorizedQuestionsTool : AgentToolBase<GetUncategorizedQuestionsInput>
{
    private readonly IQuestionRepository _questionRepository;

    public GetUncategorizedQuestionsTool(
        IQuestionRepository questionRepository,
        ILogger<GetUncategorizedQuestionsTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "get_uncategorized_questions";

    public override string Description =>
        @"Retrieves interview questions that don't have a category assigned.

Use this tool to:
- Find questions that need to be categorized
- Identify questions without a categoryId
- Prepare for categorization tasks

Returns a list of questions without categories.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""limit"": {
      ""type"": ""number"",
      ""description"": ""Optional: Maximum number of questions to return (default: 50, max: 1000)"",
      ""minimum"": 1,
      ""maximum"": 1000
    }
  }
}";

    protected override GetUncategorizedQuestionsInput ValidateAndParse(JsonElement input)
    {
        var validator = new GetUncategorizedQuestionsInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        GetUncategorizedQuestionsInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        // CRITICAL: Always filter by userId for security
        var allQuestions = await _questionRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        // Filter for uncategorized questions (no categoryId and active)
        var uncategorizedQuestions = allQuestions
            .Where(q => q.IsActive && string.IsNullOrEmpty(q.CategoryId))
            .Take(input.Limit ?? 50)
            .ToList();

        return new
        {
            success = true,
            count = uncategorizedQuestions.Count,
            questions = uncategorizedQuestions.Select(q => new
            {
                id = q.Id,
                questionText = q.QuestionText,
                answer = q.Answer,
                answerPl = q.AnswerPl,
                qualificationId = q.QualificationId,
                qualificationName = q.QualificationName,
                tags = q.Tags,
                createdAt = q.CreatedAt
            })
        };
    }
}

/// <summary>
/// Input parameters for GetUncategorizedQuestionsTool
/// </summary>
public class GetUncategorizedQuestionsInput
{
    public int? Limit { get; set; }
}

/// <summary>
/// Validator for GetUncategorizedQuestionsInput
/// </summary>
public class GetUncategorizedQuestionsInputValidator : AbstractValidator<GetUncategorizedQuestionsInput>
{
    public GetUncategorizedQuestionsInputValidator()
    {
        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .InclusiveBetween(1, 1000)
                .WithMessage("Limit must be between 1 and 1000");
        });
    }
}
