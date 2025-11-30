namespace QuestionRandomizer.UnitTests.Commands.Categories;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Categories.UpdateCategory;

/// <summary>
/// Unit tests for UpdateCategoryCommandValidator
/// </summary>
public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator;

    public UpdateCategoryCommandValidatorTests()
    {
        _validator = new UpdateCategoryCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "category123",
            Name = "Updated Category"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "",
            Name = "Valid name"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Category ID is required");
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "category123",
            Name = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Category name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "category123",
            Name = new string('a', 101) // 101 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Category name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_NameExactly100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "category123",
            Name = new string('a', 100) // Exactly 100 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "",
            Name = new string('a', 101)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
