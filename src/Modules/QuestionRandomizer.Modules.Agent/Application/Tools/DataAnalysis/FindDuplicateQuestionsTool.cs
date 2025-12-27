namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataAnalysis;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for finding potential duplicate questions using text similarity
/// </summary>
public class FindDuplicateQuestionsTool : AgentToolBase<FindDuplicateQuestionsInput>
{
    private readonly IQuestionRepository _questionRepository;

    public FindDuplicateQuestionsTool(
        IQuestionRepository questionRepository,
        ILogger<FindDuplicateQuestionsTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "find_duplicate_questions";

    public override string Description =>
        @"Finds potential duplicate questions using text similarity analysis.

Use this tool to:
- Identify duplicate or very similar questions
- Find questions that should be merged
- Clean up redundant questions

Uses Jaccard similarity to compare question text.
Returns pairs of similar questions with similarity scores.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""threshold"": {
      ""type"": ""number"",
      ""description"": ""Similarity threshold (0.0-1.0, default: 0.7)"",
      ""minimum"": 0.0,
      ""maximum"": 1.0
    },
    ""limit"": {
      ""type"": ""number"",
      ""description"": ""Maximum number of duplicate pairs to return (default: 20)"",
      ""minimum"": 1,
      ""maximum"": 100
    }
  }
}";

    protected override FindDuplicateQuestionsInput ValidateAndParse(JsonElement input)
    {
        var validator = new FindDuplicateQuestionsInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        FindDuplicateQuestionsInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var questions = await _questionRepository.GetByUserIdAsync(userId, cancellationToken);
        var activeQuestions = questions.Where(q => q.IsActive).ToList();

        var threshold = input.Threshold ?? 0.7;
        var duplicatePairs = new List<object>();

        for (int i = 0; i < activeQuestions.Count && duplicatePairs.Count < (input.Limit ?? 20); i++)
        {
            for (int j = i + 1; j < activeQuestions.Count && duplicatePairs.Count < (input.Limit ?? 20); j++)
            {
                var similarity = CalculateJaccardSimilarity(
                    activeQuestions[i].QuestionText,
                    activeQuestions[j].QuestionText);

                if (similarity >= threshold)
                {
                    duplicatePairs.Add(new
                    {
                        question1 = new
                        {
                            id = activeQuestions[i].Id,
                            questionText = activeQuestions[i].QuestionText
                        },
                        question2 = new
                        {
                            id = activeQuestions[j].Id,
                            questionText = activeQuestions[j].QuestionText
                        },
                        similarity = Math.Round(similarity, 3)
                    });
                }
            }
        }

        return new
        {
            success = true,
            count = duplicatePairs.Count,
            threshold,
            duplicatePairs
        };
    }

    /// <summary>
    /// Calculates Jaccard similarity between two texts
    /// </summary>
    private static double CalculateJaccardSimilarity(string text1, string text2)
    {
        var words1 = text1.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '?', '!', ';', ':', '\n', '\r' },
                   StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var words2 = text2.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '?', '!', ';', ':', '\n', '\r' },
                   StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        if (words1.Count == 0 && words2.Count == 0)
            return 1.0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return union > 0 ? (double)intersection / union : 0.0;
    }
}

public class FindDuplicateQuestionsInput
{
    public double? Threshold { get; set; }
    public int? Limit { get; set; }
}

public class FindDuplicateQuestionsInputValidator : AbstractValidator<FindDuplicateQuestionsInput>
{
    public FindDuplicateQuestionsInputValidator()
    {
        When(x => x.Threshold.HasValue, () =>
        {
            RuleFor(x => x.Threshold!.Value)
                .InclusiveBetween(0.0, 1.0)
                .WithMessage("Threshold must be between 0.0 and 1.0");
        });

        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit must be between 1 and 100");
        });
    }
}
