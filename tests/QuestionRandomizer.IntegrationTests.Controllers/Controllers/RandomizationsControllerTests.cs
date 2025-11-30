namespace QuestionRandomizer.IntegrationTests.Controllers.Controllers;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Randomizations.CreateRandomization;
using QuestionRandomizer.Application.Commands.Randomizations.UpdateRandomization;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.IntegrationTests.Controllers.Infrastructure;

/// <summary>
/// Integration tests for RandomizationsController
/// </summary>
public class RandomizationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RandomizationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.ResetMocks();
    }

    [Fact]
    public async Task GetRandomization_ExistingRandomization_ReturnsOk()
    {
        // Arrange
        var randomization = new Randomization
        {
            Id = "test-randomization-id",
            UserId = CustomWebApplicationFactory.TestUserId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = "question-123",
            CreatedAt = DateTime.UtcNow
        };

        _factory.RandomizationRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(randomization);

        // Act
        var response = await _client.GetAsync("/api/randomizations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RandomizationDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be("test-randomization-id");
        result.Status.Should().Be("Ongoing");
        result.CurrentQuestionId.Should().Be("question-123");
    }

    [Fact]
    public async Task GetRandomization_NoActiveRandomization_ReturnsNotFound()
    {
        // Arrange
        _factory.RandomizationRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Randomization?)null);

        // Act
        var response = await _client.GetAsync("/api/randomizations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateRandomization_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateRandomizationCommand
        {
        };

        var createdRandomization = new Randomization
        {
            Id = "new-randomization-id",
            UserId = CustomWebApplicationFactory.TestUserId,
            ShowAnswer = false,
            Status = "Ongoing",
            CreatedAt = DateTime.UtcNow
        };

        _factory.RandomizationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRandomization);

        // Act
        var response = await _client.PostAsJsonAsync("/api/randomizations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<RandomizationDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-randomization-id");
        result.Status.Should().Be("Ongoing");

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/Randomizations");
    }

    [Fact]
    public async Task UpdateRandomization_ValidCommand_ReturnsOk()
    {
        // Arrange
        var randomizationId = "randomization-to-update";
        var existingRandomization = new Randomization
        {
            Id = randomizationId,
            UserId = CustomWebApplicationFactory.TestUserId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = "question-1",
            CreatedAt = DateTime.UtcNow
        };

        _factory.RandomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                randomizationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRandomization);

        _factory.RandomizationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = true,
            Status = "Completed",
            CurrentQuestionId = "question-2"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/randomizations/{randomizationId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RandomizationDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(randomizationId);
        result.ShowAnswer.Should().BeTrue();
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task UpdateRandomization_NonExistentRandomization_ReturnsNotFound()
    {
        // Arrange
        var randomizationId = "non-existent-id";

        _factory.RandomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                randomizationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Randomization?)null);

        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = true,
            Status = "Ongoing",
            CurrentQuestionId = null
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/randomizations/{randomizationId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRandomization_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var randomizationId = "randomization-1";
        var command = new UpdateRandomizationCommand
        {
            Id = "randomization-2", // Different from URL
            ShowAnswer = true,
            Status = "Ongoing",
            CurrentQuestionId = null
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/randomizations/{randomizationId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ClearCurrentQuestion_ExistingRandomization_ReturnsNoContent()
    {
        // Arrange
        var randomizationId = "test-randomization-id";

        _factory.RandomizationRepositoryMock
            .Setup(x => x.ClearCurrentQuestionAsync(
                randomizationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.PostAsync($"/api/randomizations/{randomizationId}/clear-current-question", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _factory.RandomizationRepositoryMock.Verify(
            x => x.ClearCurrentQuestionAsync(
                randomizationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ClearCurrentQuestion_NonExistentRandomization_ReturnsNotFound()
    {
        // Arrange
        var randomizationId = "non-existent-id";

        _factory.RandomizationRepositoryMock
            .Setup(x => x.ClearCurrentQuestionAsync(
                randomizationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _client.PostAsync($"/api/randomizations/{randomizationId}/clear-current-question", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
