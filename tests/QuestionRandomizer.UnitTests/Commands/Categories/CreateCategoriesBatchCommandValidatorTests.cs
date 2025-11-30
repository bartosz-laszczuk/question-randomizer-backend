namespace QuestionRandomizer.UnitTests.Commands.Categories;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Categories.CreateCategoriesBatch;

/// <summary>
/// Unit tests for CreateCategoriesBatchCommandValidator
/// </summary>
public class CreateCategoriesBatchCommandValidatorTests
{
    private readonly CreateCategoriesBatchCommandValidator _validator;

    public CreateCategoriesBatchCommandValidatorTests()
    {
        _validator = new CreateCategoriesBatchCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "Category 1", "Category 2", "Category 3" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyCategoryNames_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryNames)
            .WithErrorMessage("At least one category name is required");
    }

    [Fact]
    public void Validate_TooManyCategoryNames_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = Enumerable.Range(1, 101).Select(i => $"Category {i}").ToList() // 101 categories
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryNames)
            .WithErrorMessage("Maximum 100 categories can be created at once");
    }

    [Fact]
    public void Validate_Exactly100CategoryNames_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = Enumerable.Range(1, 100).Select(i => $"Category {i}").ToList() // Exactly 100 categories
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryNames);
    }

    [Fact]
    public void Validate_EmptyCategoryNameInList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "Category 1", "", "Category 3" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("CategoryNames[1]")
            .WithErrorMessage("Category name cannot be empty");
    }

    [Fact]
    public void Validate_CategoryNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "Category 1", new string('a', 101), "Category 3" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("CategoryNames[1]")
            .WithErrorMessage("Category name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_CategoryNameExactly100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { new string('a', 100) }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryNames);
    }
}
