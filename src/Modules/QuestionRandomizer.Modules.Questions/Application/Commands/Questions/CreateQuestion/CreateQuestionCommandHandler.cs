namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.CreateQuestion;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, QuestionDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ICategoryRepository categoryRepository,
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _categoryRepository = categoryRepository;
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QuestionDto> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Get category name if categoryId provided
        string? categoryName = null;
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, userId, cancellationToken);
            categoryName = category?.Name;
        }

        // Get qualification name if qualificationId provided
        string? qualificationName = null;
        if (!string.IsNullOrEmpty(request.QualificationId))
        {
            var qualification = await _qualificationRepository.GetByIdAsync(request.QualificationId, userId, cancellationToken);
            qualificationName = qualification?.Name;
        }

        var question = new Question
        {
            QuestionText = request.QuestionText,
            Answer = request.Answer,
            AnswerPl = request.AnswerPl,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            QualificationId = request.QualificationId,
            QualificationName = qualificationName,
            Tags = request.Tags,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdQuestion = await _questionRepository.CreateAsync(question, cancellationToken);

        return new QuestionDto
        {
            Id = createdQuestion.Id,
            QuestionText = createdQuestion.QuestionText,
            Answer = createdQuestion.Answer,
            AnswerPl = createdQuestion.AnswerPl,
            CategoryId = createdQuestion.CategoryId,
            CategoryName = createdQuestion.CategoryName,
            QualificationId = createdQuestion.QualificationId,
            QualificationName = createdQuestion.QualificationName,
            IsActive = createdQuestion.IsActive,
            Tags = createdQuestion.Tags,
            CreatedAt = createdQuestion.CreatedAt,
            UpdatedAt = createdQuestion.UpdatedAt
        };
    }
}
