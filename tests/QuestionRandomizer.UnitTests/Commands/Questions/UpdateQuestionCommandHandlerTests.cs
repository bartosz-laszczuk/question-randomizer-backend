namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.UpdateQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for UpdateQuestionCommandHandler
/// </summary>
public class UpdateQuestionCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateQuestionCommandHandler _handler;

    public UpdateQuestionCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdateQuestionCommandHandler(
            _questionRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesQuestionSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-456";
        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Updated question text?",
            Answer = "Updated answer text",
            AnswerPl = "Zaktualizowana odpowiedź",
            Tags = new List<string> { "updated", "test" }
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old question text",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(questionId);
        result.QuestionText.Should().Be(command.QuestionText);
        result.Answer.Should().Be(command.Answer);
        result.AnswerPl.Should().Be(command.AnswerPl);
        result.Tags.Should().BeEquivalentTo(command.Tags);
        result.IsActive.Should().BeTrue();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Question>(q =>
                    q.Id == questionId &&
                    q.QuestionText == command.QuestionText &&
                    q.Answer == command.Answer &&
                    q.AnswerPl == command.AnswerPl),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "non-existent-question";
        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Updated text",
            Answer = "Updated answer",
            AnswerPl = "Zaktualizowana odpowiedź"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Question with ID '{questionId}' was not found");

        _questionRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CommandWithCategoryId_UpdatesCategoryName()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-789";
        var categoryId = "category-456";
        var categoryName = "Software Engineering";

        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Question with category",
            Answer = "Answer",
            AnswerPl = "Odpowiedź",
            CategoryId = categoryId
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old text",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var category = new Category
        {
            Id = categoryId,
            Name = categoryName,
            UserId = userId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(categoryName);

        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Question>(q =>
                    q.CategoryId == categoryId &&
                    q.CategoryName == categoryName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithQualificationId_UpdatesQualificationName()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-999";
        var qualificationId = "qual-789";
        var qualificationName = "Azure Developer Associate";

        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Question with qualification",
            Answer = "Answer",
            AnswerPl = "Odpowiedź",
            QualificationId = qualificationId
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old text",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var qualification = new Qualification
        {
            Id = qualificationId,
            Name = qualificationName,
            UserId = userId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.QualificationId.Should().Be(qualificationId);
        result.QualificationName.Should().Be(qualificationName);

        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Question>(q =>
                    q.QualificationId == qualificationId &&
                    q.QualificationName == qualificationName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_SetsNullCategoryName()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-111";
        var categoryId = "non-existent-category";

        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Question",
            Answer = "Answer",
            AnswerPl = "Odpowiedź",
            CategoryId = categoryId
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old text",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().BeNull();

        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QualificationNotFound_SetsNullQualificationName()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-222";
        var qualificationId = "non-existent-qualification";

        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Question",
            Answer = "Answer",
            AnswerPl = "Odpowiedź",
            QualificationId = qualificationId
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old text",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Qualification?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.QualificationId.Should().Be(qualificationId);
        result.QualificationName.Should().BeNull();

        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithNullCategoryId_DoesNotFetchCategory()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-333";

        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Question",
            Answer = "Answer",
            AnswerPl = "Odpowiedź",
            CategoryId = null
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old text",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            CategoryId = "old-category",
            CategoryName = "Old Category",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().BeNull();
        result.CategoryName.Should().BeNull();

        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatedAtTimestamp_IsSetToUtcNow()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-444";
        var beforeUpdate = DateTime.UtcNow;

        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Updated question",
            Answer = "Updated answer",
            AnswerPl = "Zaktualizowana odpowiedź"
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old text",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        result.UpdatedAt.Should().BeOnOrBefore(afterUpdate);

        _questionRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Question>(q => q.UpdatedAt >= beforeUpdate && q.UpdatedAt <= afterUpdate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithAllFields_UpdatesAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-555";
        var categoryId = "category-123";
        var categoryName = "Category Name";
        var qualificationId = "qual-456";
        var qualificationName = "Qualification Name";

        var command = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "Comprehensive updated question",
            Answer = "Comprehensive updated answer",
            AnswerPl = "Wyczerpująca zaktualizowana odpowiedź",
            CategoryId = categoryId,
            QualificationId = qualificationId,
            Tags = new List<string> { "tag1", "tag2", "tag3" }
        };

        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old question",
            Answer = "Old answer",
            AnswerPl = "Stara odpowiedź",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        var category = new Category { Id = categoryId, Name = categoryName, UserId = userId };
        var qualification = new Qualification { Id = qualificationId, Name = qualificationName, UserId = userId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(questionId);
        result.QuestionText.Should().Be(command.QuestionText);
        result.Answer.Should().Be(command.Answer);
        result.AnswerPl.Should().Be(command.AnswerPl);
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(categoryName);
        result.QualificationId.Should().Be(qualificationId);
        result.QualificationName.Should().Be(qualificationName);
        result.Tags.Should().BeEquivalentTo(command.Tags);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
