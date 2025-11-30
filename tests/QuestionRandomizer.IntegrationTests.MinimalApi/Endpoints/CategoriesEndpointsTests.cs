namespace QuestionRandomizer.IntegrationTests.MinimalApi.Endpoints;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Categories.CreateCategory;
using QuestionRandomizer.Application.Commands.Categories.CreateCategoriesBatch;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.IntegrationTests.MinimalApi.Infrastructure;

/// <summary>
/// Integration tests for Categories endpoints (Minimal API)
/// </summary>
public class CategoriesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoriesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.ResetMocks();
    }

    [Fact]
    public async Task GetCategories_ReturnsOkWithCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new()
            {
                Id = "cat1",
                Name = "Software Engineering",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "cat2",
                Name = "Cloud Computing",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _factory.CategoryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Id.Should().Be("cat1");
        result[1].Id.Should().Be("cat2");
    }

    [Fact]
    public async Task GetCategoryById_ExistingCategory_ReturnsOk()
    {
        // Arrange
        var categoryId = "test-category-id";
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var response = await _client.GetAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetCategoryById_NonExistentCategory_ReturnsNotFound()
    {
        // Arrange
        var categoryId = "non-existent-id";

        _factory.CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var response = await _client.GetAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "New Category"
        };

        var createdCategory = new Category
        {
            Id = "new-category-id",
            Name = command.Name,
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.CategoryRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-category-id");
        result.Name.Should().Be(command.Name);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/categories/{result.Id}");
    }

    [Fact]
    public async Task CreateCategory_InvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "" // Invalid - empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategoriesBatch_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "Category 1", "Category 2", "Category 3" }
        };

        var createdCategories = new List<Category>
        {
            new() { Id = "cat1", Name = "Category 1", UserId = CustomWebApplicationFactory.TestUserId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = "cat2", Name = "Category 2", UserId = CustomWebApplicationFactory.TestUserId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = "cat3", Name = "Category 3", UserId = CustomWebApplicationFactory.TestUserId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _factory.CategoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategories);

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories/batch", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result![0].Name.Should().Be("Category 1");
        result[1].Name.Should().Be("Category 2");
        result[2].Name.Should().Be("Category 3");
    }

    [Fact]
    public async Task UpdateCategory_ValidCommand_ReturnsOk()
    {
        // Arrange
        var categoryId = "category-to-update";
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Category",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _factory.CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _factory.CategoryRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new { Id = categoryId, Name = "Updated Category" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{categoryId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Category");
    }

    [Fact]
    public async Task UpdateCategory_NonExistentCategory_ReturnsNotFound()
    {
        // Arrange
        var categoryId = "non-existent-id";

        _factory.CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new { Id = categoryId, Name = "Updated Category" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{categoryId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_ExistingCategory_ReturnsNoContent()
    {
        // Arrange
        var categoryId = "category-to-delete";

        _factory.CategoryRepositoryMock
            .Setup(x => x.DeleteAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _factory.CategoryRepositoryMock.Verify(
            x => x.DeleteAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_NonExistentCategory_ReturnsNotFound()
    {
        // Arrange
        var categoryId = "non-existent-id";

        _factory.CategoryRepositoryMock
            .Setup(x => x.DeleteAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
