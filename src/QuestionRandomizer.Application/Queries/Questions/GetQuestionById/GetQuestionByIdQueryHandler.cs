namespace QuestionRandomizer.Application.Queries.Questions.GetQuestionById;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;
using QuestionRandomizer.Domain.Exceptions;

/// <summary>
/// Handler for GetQuestionByIdQuery
/// </summary>
public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, QuestionDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQuestionByIdQueryHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QuestionDto> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var question = await _questionRepository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (question == null)
        {
            throw new NotFoundException("Question", request.Id);
        }

        return new QuestionDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            Answer = question.Answer,
            AnswerPl = question.AnswerPl,
            CategoryId = question.CategoryId,
            CategoryName = question.CategoryName,
            QualificationId = question.QualificationId,
            QualificationName = question.QualificationName,
            IsActive = question.IsActive,
            Tags = question.Tags,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt
        };
    }
}
