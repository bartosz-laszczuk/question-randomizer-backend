namespace QuestionRandomizer.Application.Commands.Categories.CreateCategoriesBatch;

using FluentValidation;

/// <summary>
/// Validator for CreateCategoriesBatchCommand
/// </summary>
public class CreateCategoriesBatchCommandValidator : AbstractValidator<CreateCategoriesBatchCommand>
{
    public CreateCategoriesBatchCommandValidator()
    {
        RuleFor(x => x.CategoryNames)
            .NotEmpty().WithMessage("At least one category name is required")
            .Must(names => names.Count <= 100).WithMessage("Maximum 100 categories can be created at once");

        RuleForEach(x => x.CategoryNames)
            .NotEmpty().WithMessage("Category name cannot be empty")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");
    }
}
