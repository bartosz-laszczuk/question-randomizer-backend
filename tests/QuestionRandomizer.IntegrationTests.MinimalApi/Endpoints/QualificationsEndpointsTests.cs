namespace QuestionRandomizer.IntegrationTests.MinimalApi.Endpoints;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualification;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualificationsBatch;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.IntegrationTests.MinimalApi.Infrastructure;

/// <summary>
/// Integration tests for Qualifications endpoints (Minimal API)
/// </summary>
public class QualificationsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QualificationsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.ResetMocks();
    }

    [Fact]
    public async Task GetQualifications_ReturnsOkWithQualifications()
    {
        // Arrange
        var qualifications = new List<Qualification>
        {
            new()
            {
                Id = "qual1",
                Name = "Azure Developer Associate",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "qual2",
                Name = "AWS Solutions Architect",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _factory.QualificationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualifications);

        // Act
        var response = await _client.GetAsync("/api/qualifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<QualificationDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Id.Should().Be("qual1");
        result[1].Id.Should().Be("qual2");
    }

    [Fact]
    public async Task GetQualificationById_ExistingQualification_ReturnsOk()
    {
        // Arrange
        var qualificationId = "test-qual-id";
        var qualification = new Qualification
        {
            Id = qualificationId,
            Name = "Test Qualification",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.QualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                qualificationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualification);

        // Act
        var response = await _client.GetAsync($"/api/qualifications/{qualificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<QualificationDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(qualificationId);
        result.Name.Should().Be("Test Qualification");
    }

    [Fact]
    public async Task GetQualificationById_NonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = "non-existent-id";

        _factory.QualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                qualificationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Qualification?)null);

        // Act
        var response = await _client.GetAsync($"/api/qualifications/{qualificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateQualification_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateQualificationCommand
        {
            Name = "New Qualification"
        };

        var createdQualification = new Qualification
        {
            Id = "new-qual-id",
            Name = command.Name,
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.QualificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualification);

        // Act
        var response = await _client.PostAsJsonAsync("/api/qualifications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<QualificationDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-qual-id");
        result.Name.Should().Be(command.Name);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/qualifications/{result.Id}");
    }

    [Fact]
    public async Task CreateQualification_InvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateQualificationCommand
        {
            Name = "" // Invalid - empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/qualifications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateQualificationsBatch_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "Qualification 1", "Qualification 2", "Qualification 3" }
        };

        var createdQualifications = new List<Qualification>
        {
            new() { Id = "qual1", Name = "Qualification 1", UserId = CustomWebApplicationFactory.TestUserId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = "qual2", Name = "Qualification 2", UserId = CustomWebApplicationFactory.TestUserId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = "qual3", Name = "Qualification 3", UserId = CustomWebApplicationFactory.TestUserId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _factory.QualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualifications);

        // Act
        var response = await _client.PostAsJsonAsync("/api/qualifications/batch", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<List<QualificationDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result![0].Name.Should().Be("Qualification 1");
        result[1].Name.Should().Be("Qualification 2");
        result[2].Name.Should().Be("Qualification 3");
    }

    [Fact]
    public async Task UpdateQualification_ValidCommand_ReturnsOk()
    {
        // Arrange
        var qualificationId = "qual-to-update";
        var existingQualification = new Qualification
        {
            Id = qualificationId,
            Name = "Old Qualification",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _factory.QualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                qualificationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQualification);

        _factory.QualificationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new { Id = qualificationId, Name = "Updated Qualification" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/qualifications/{qualificationId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<QualificationDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Qualification");
    }

    [Fact]
    public async Task UpdateQualification_NonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = "non-existent-id";

        _factory.QualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                qualificationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Qualification?)null);

        var command = new { Id = qualificationId, Name = "Updated Qualification" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/qualifications/{qualificationId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQualification_ExistingQualification_ReturnsNoContent()
    {
        // Arrange
        var qualificationId = "qual-to-delete";

        _factory.QualificationRepositoryMock
            .Setup(x => x.DeleteAsync(
                qualificationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync($"/api/qualifications/{qualificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _factory.QualificationRepositoryMock.Verify(
            x => x.DeleteAsync(
                qualificationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteQualification_NonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = "non-existent-id";

        _factory.QualificationRepositoryMock
            .Setup(x => x.DeleteAsync(
                qualificationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync($"/api/qualifications/{qualificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
