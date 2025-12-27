namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for searching questions by text content
/// </summary>
public class SearchQuestionsTool : AgentToolBase<SearchQuestionsInput>
{
    private readonly IQuestionRepository _questionRepository;

    public SearchQuestionsTool(
        IQuestionRepository questionRepository,
        ILogger<SearchQuestionsTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "search_questions";

    public override string Description =>
        @"Searches questions by text in question or answer fields.

Use this tool to:
- Find questions containing specific keywords
- Search for questions about a topic
- Locate questions by partial text match

Returns matching questions.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""searchText"": {
      ""type"": ""string"",
      ""description"": ""Text to search for in questions and answers""
    },
    ""limit"": {
      ""type"": ""number"",
      ""description"": ""Maximum number of results (default: 50, max: 200)"",
      ""minimum"": 1,
      ""maximum"": 200
    }
  },
  ""required"": [""searchText""]
}";

    protected override SearchQuestionsInput ValidateAndParse(JsonElement input)
    {
        var validator = new SearchQuestionsInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        SearchQuestionsInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var allQuestions = await _questionRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        var searchLower = input.SearchText.ToLowerInvariant();
        var matchingQuestions = allQuestions
            .Where(q => q.IsActive &&
                       (q.QuestionText.ToLowerInvariant().Contains(searchLower) ||
                        q.Answer.ToLowerInvariant().Contains(searchLower) ||
                        q.AnswerPl.ToLowerInvariant().Contains(searchLower) ||
                        (q.Tags != null && q.Tags.Any(t => t.ToLowerInvariant().Contains(searchLower)))))
            .Take(input.Limit ?? 50)
            .ToList();

        return new
        {
            success = true,
            count = matchingQuestions.Count,
            searchText = input.SearchText,
            questions = matchingQuestions.Select(q => new
            {
                id = q.Id,
                questionText = q.QuestionText,
                answer = q.Answer,
                categoryName = q.CategoryName,
                tags = q.Tags
            })
        };
    }
}

public class SearchQuestionsInput
{
    public string SearchText { get; set; } = string.Empty;
    public int? Limit { get; set; }
}

public class SearchQuestionsInputValidator : AbstractValidator<SearchQuestionsInput>
{
    public SearchQuestionsInputValidator()
    {
        RuleFor(x => x.SearchText)
            .NotEmpty()
            .WithMessage("Search text is required")
            .MinimumLength(2)
            .WithMessage("Search text must be at least 2 characters");

        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .InclusiveBetween(1, 200)
                .WithMessage("Limit must be between 1 and 200");
        });
    }
}
