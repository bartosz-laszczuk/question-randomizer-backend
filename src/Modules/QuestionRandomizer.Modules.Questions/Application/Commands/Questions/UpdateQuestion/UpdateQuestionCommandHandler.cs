namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.UpdateQuestion;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Domain.Exceptions;

/// <summary>
/// Handler for UpdateQuestionCommand
/// </summary>
public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, QuestionDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateQuestionCommandHandler(
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

    public async Task<QuestionDto> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Get existing question
        var existingQuestion = await _questionRepository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (existingQuestion == null)
        {
            throw new NotFoundException("Question", request.Id);
        }

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

        existingQuestion.QuestionText = request.QuestionText;
        existingQuestion.Answer = request.Answer;
        existingQuestion.AnswerPl = request.AnswerPl;
        existingQuestion.CategoryId = request.CategoryId;
        existingQuestion.CategoryName = categoryName;
        existingQuestion.QualificationId = request.QualificationId;
        existingQuestion.QualificationName = qualificationName;
        existingQuestion.Tags = request.Tags;
        existingQuestion.UpdatedAt = DateTime.UtcNow;

        await _questionRepository.UpdateAsync(existingQuestion, cancellationToken);

        return new QuestionDto
        {
            Id = existingQuestion.Id,
            QuestionText = existingQuestion.QuestionText,
            Answer = existingQuestion.Answer,
            AnswerPl = existingQuestion.AnswerPl,
            CategoryId = existingQuestion.CategoryId,
            CategoryName = existingQuestion.CategoryName,
            QualificationId = existingQuestion.QualificationId,
            QualificationName = existingQuestion.QualificationName,
            IsActive = existingQuestion.IsActive,
            Tags = existingQuestion.Tags,
            CreatedAt = existingQuestion.CreatedAt,
            UpdatedAt = existingQuestion.UpdatedAt
        };
    }
}
