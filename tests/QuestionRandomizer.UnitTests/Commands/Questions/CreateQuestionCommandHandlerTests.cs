namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateQuestionCommandHandler
/// </summary>
public class CreateQuestionCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateQuestionCommandHandler _handler;

    public CreateQuestionCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateQuestionCommandHandler(
            _questionRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesQuestionSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQuestionCommand
        {
            QuestionText = "What is dependency injection?",
            Answer = "Dependency injection is a design pattern...",
            AnswerPl = "Wstrzykiwanie zależności to wzorzec projektowy...",
            Tags = new List<string> { "design-patterns", "architecture" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQuestion = new Question
        {
            Id = "question-123",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            UserId = userId,
            IsActive = true,
            Tags = command.Tags,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("question-123");
        result.QuestionText.Should().Be(command.QuestionText);
        result.Answer.Should().Be(command.Answer);
        result.AnswerPl.Should().Be(command.AnswerPl);
        result.IsActive.Should().BeTrue();
        result.Tags.Should().BeEquivalentTo(command.Tags);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Question>(q =>
                    q.QuestionText == command.QuestionText &&
                    q.Answer == command.Answer &&
                    q.AnswerPl == command.AnswerPl &&
                    q.UserId == userId &&
                    q.IsActive == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithCategoryId_FetchesCategoryNameAndCreatesQuestion()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var categoryName = "Software Engineering";

        var command = new CreateQuestionCommand
        {
            QuestionText = "What is SOLID?",
            Answer = "SOLID is a set of principles...",
            AnswerPl = "SOLID to zestaw zasad...",
            CategoryId = categoryId
        };

        var category = new Category
        {
            Id = categoryId,
            Name = categoryName,
            UserId = userId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var createdQuestion = new Question
        {
            Id = "question-789",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            CategoryId = categoryId,
            CategoryName = categoryName,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

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
            x => x.CreateAsync(
                It.Is<Question>(q =>
                    q.CategoryId == categoryId &&
                    q.CategoryName == categoryName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithQualificationId_FetchesQualificationNameAndCreatesQuestion()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-789";
        var qualificationName = "Azure Developer Associate";

        var command = new CreateQuestionCommand
        {
            QuestionText = "What is Azure Functions?",
            Answer = "Azure Functions is a serverless compute service...",
            AnswerPl = "Azure Functions to usługa obliczeń bezserwerowych...",
            QualificationId = qualificationId
        };

        var qualification = new Qualification
        {
            Id = qualificationId,
            Name = qualificationName,
            UserId = userId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualification);

        var createdQuestion = new Question
        {
            Id = "question-999",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            QualificationId = qualificationId,
            QualificationName = qualificationName,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

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
            x => x.CreateAsync(
                It.Is<Question>(q =>
                    q.QualificationId == qualificationId &&
                    q.QualificationName == qualificationName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithNullCategoryId_DoesNotFetchCategory()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQuestionCommand
        {
            QuestionText = "Test question",
            Answer = "Test answer",
            AnswerPl = "Testowa odpowiedź",
            CategoryId = null
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQuestion = new Question
        {
            Id = "question-111",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

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
    public async Task Handle_CategoryNotFound_CreatesQuestionWithNullCategoryName()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "non-existent-category";

        var command = new CreateQuestionCommand
        {
            QuestionText = "Test question",
            Answer = "Test answer",
            AnswerPl = "Testowa odpowiedź",
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var createdQuestion = new Question
        {
            Id = "question-222",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            CategoryId = categoryId,
            CategoryName = null,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

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
    public async Task Handle_QualificationNotFound_CreatesQuestionWithNullQualificationName()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "non-existent-qualification";

        var command = new CreateQuestionCommand
        {
            QuestionText = "Test question",
            Answer = "Test answer",
            AnswerPl = "Testowa odpowiedź",
            QualificationId = qualificationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Qualification?)null);

        var createdQuestion = new Question
        {
            Id = "question-333",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            QualificationId = qualificationId,
            QualificationName = null,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

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
    public async Task Handle_CommandWithAllFields_CreatesCompleteQuestion()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var categoryName = "Programming";
        var qualificationId = "qual-789";
        var qualificationName = "Senior Developer";

        var command = new CreateQuestionCommand
        {
            QuestionText = "Comprehensive question",
            Answer = "Comprehensive answer",
            AnswerPl = "Wyczerpująca odpowiedź",
            CategoryId = categoryId,
            QualificationId = qualificationId,
            Tags = new List<string> { "tag1", "tag2", "tag3" }
        };

        var category = new Category { Id = categoryId, Name = categoryName, UserId = userId };
        var qualification = new Qualification { Id = qualificationId, Name = qualificationName, UserId = userId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualification);

        var createdQuestion = new Question
        {
            Id = "question-444",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            CategoryId = categoryId,
            CategoryName = categoryName,
            QualificationId = qualificationId,
            QualificationName = qualificationName,
            Tags = command.Tags,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("question-444");
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
            x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
