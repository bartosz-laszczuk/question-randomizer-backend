namespace QuestionRandomizer.UnitTests.Commands.UsedQuestions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.UsedQuestions.AddUsedQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for AddUsedQuestionCommandHandler
/// </summary>
public class AddUsedQuestionCommandHandlerTests
{
    private readonly Mock<IUsedQuestionRepository> _usedQuestionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AddUsedQuestionCommandHandler _handler;

    public AddUsedQuestionCommandHandlerTests()
    {
        _usedQuestionRepositoryMock = new Mock<IUsedQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new AddUsedQuestionCommandHandler(
            _usedQuestionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsUsedQuestionSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var questionId = "question-789";
        var categoryId = "category-111";
        var categoryName = "Software Engineering";

        var command = new AddUsedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdUsedQuestion = new UsedQuestion
        {
            Id = "used-999",
            QuestionId = questionId,
            CategoryId = categoryId,
            CategoryName = categoryName,
            CreatedAt = DateTime.UtcNow
        };

        _usedQuestionRepositoryMock
            .Setup(x => x.AddAsync(randomizationId, userId, It.IsAny<UsedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUsedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("used-999");
        result.QuestionId.Should().Be(questionId);
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(categoryName);
        result.CreatedAt.Should().NotBe(default);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _usedQuestionRepositoryMock.Verify(
            x => x.AddAsync(
                randomizationId,
                userId,
                It.Is<UsedQuestion>(uq =>
                    uq.QuestionId == questionId &&
                    uq.CategoryId == categoryId &&
                    uq.CategoryName == categoryName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCreatedAtTimestamp()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new AddUsedQuestionCommand
        {
            RandomizationId = "random-123",
            QuestionId = "question-456",
            CategoryId = "category-789",
            CategoryName = "Test Category"
        };

        var beforeCreation = DateTime.UtcNow;

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdUsedQuestion = new UsedQuestion
        {
            Id = "used-1",
            QuestionId = command.QuestionId,
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _usedQuestionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UsedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUsedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterCreation = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeAfter(beforeCreation.AddSeconds(-1));
        result.CreatedAt.Should().BeBefore(afterCreation.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new AddUsedQuestionCommand
        {
            RandomizationId = "random-123",
            QuestionId = "question-456",
            CategoryId = "category-789",
            CategoryName = "Test Category"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdUsedQuestion = new UsedQuestion
        {
            Id = "used-1",
            QuestionId = command.QuestionId,
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _usedQuestionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), userId, It.IsAny<UsedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUsedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _usedQuestionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<string>(), userId, It.IsAny<UsedQuestion>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var cancellationToken = new CancellationToken();

        var command = new AddUsedQuestionCommand
        {
            RandomizationId = "random-123",
            QuestionId = "question-456",
            CategoryId = "category-789",
            CategoryName = "Test Category"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdUsedQuestion = new UsedQuestion
        {
            Id = "used-1",
            QuestionId = command.QuestionId,
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _usedQuestionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UsedQuestion>(), cancellationToken))
            .ReturnsAsync(createdUsedQuestion);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _usedQuestionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UsedQuestion>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_PassesCorrectRandomizationId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "unique-random-id-789";

        var command = new AddUsedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = "question-456",
            CategoryId = "category-789",
            CategoryName = "Test Category"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdUsedQuestion = new UsedQuestion
        {
            Id = "used-1",
            QuestionId = command.QuestionId,
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _usedQuestionRepositoryMock
            .Setup(x => x.AddAsync(randomizationId, It.IsAny<string>(), It.IsAny<UsedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUsedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _usedQuestionRepositoryMock.Verify(
            x => x.AddAsync(randomizationId, It.IsAny<string>(), It.IsAny<UsedQuestion>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
