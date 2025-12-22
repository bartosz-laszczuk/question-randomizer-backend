namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.CreateCategory;

using FluentValidation;

/// <summary>
/// Validator for CreateCategoryCommand
/// </summary>
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");
    }
}
