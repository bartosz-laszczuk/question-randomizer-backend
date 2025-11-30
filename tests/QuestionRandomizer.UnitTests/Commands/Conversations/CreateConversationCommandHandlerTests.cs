namespace QuestionRandomizer.UnitTests.Commands.Conversations;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Conversations.CreateConversation;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateConversationCommandHandler
/// </summary>
public class CreateConversationCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateConversationCommandHandler _handler;

    public CreateConversationCommandHandlerTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateConversationCommandHandler(
            _conversationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesConversationSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateConversationCommand { Title = "My Conversation" };

        var createdConversation = new Conversation
        {
            Id = "conv-456",
            Title = command.Title,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdConversation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("conv-456");
        result.Title.Should().Be(command.Title);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _conversationRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Conversation>(c =>
                    c.Title == command.Title &&
                    c.UserId == userId &&
                    c.IsActive == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SetsTimestampsToUtcNow()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateConversationCommand { Title = "Test Conversation" };
        var beforeCreate = DateTime.UtcNow;

        var createdConversation = new Conversation
        {
            Id = "conv-789",
            Title = command.Title,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdConversation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().NotBeNull();
        result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.CreatedAt.Should().BeOnOrBefore(afterCreate);
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeOnOrAfter(beforeCreate);
        result.UpdatedAt.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task Handle_UsesCurrentUserService()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new CreateConversationCommand { Title = "User Conversation" };

        var createdConversation = new Conversation
        {
            Id = "conv-111",
            Title = command.Title,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdConversation);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _conversationRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Conversation>(c => c.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
