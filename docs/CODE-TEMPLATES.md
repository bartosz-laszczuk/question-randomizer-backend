# Code Templates and Patterns

This document contains all code templates and patterns used in the Question Randomizer Backend project.

**Architecture:** Modular Monolith (migrated from Clean Architecture on 2025-12-22)
**Last Updated:** 2025-12-22

---

## Table of Contents

### Modular Monolith Patterns (NEW)
1. [Module Structure Overview](#1-module-structure-overview)
2. [Module Extension Template](#2-module-extension-template-new)
3. [Domain Event Template](#3-domain-event-template-new)
4. [Event Handler Template](#4-event-handler-template-new)
5. [Cross-Module Event Handler](#5-cross-module-event-handler-new)

### Core CQRS Patterns
6. [Domain Entity Template](#6-domain-entity-template)
7. [Repository Interface Template](#7-repository-interface-template)
8. [CQRS Command Template](#8-cqrs-command-template)
9. [Command Handler Template](#9-command-handler-template)
10. [FluentValidation Validator Template](#10-fluentvalidation-validator-template)
11. [CQRS Query Template](#11-cqrs-query-template)
12. [Query Handler Template](#12-query-handler-template)

### API Patterns
13. [Controller Template](#13-controller-template)
14. [Minimal API Endpoint Template](#14-minimal-api-endpoint-template)

### Infrastructure Patterns
15. [Repository Implementation Template](#15-repository-implementation-template)
16. [Program.cs Template (Modular)](#16-programcs-template-modular)
17. [Validation Pipeline Behavior](#17-validation-pipeline-behavior)

### Testing Patterns
18. [Unit Test Template](#18-unit-test-template)

---

## 1. Module Structure Overview

### Modular Monolith Architecture

Each module is a **vertical slice** containing all layers for a specific business capability:

```
QuestionRandomizer.Modules.<ModuleName>/
├── Domain/                              # Entities, Value Objects
│   ├── Entities/
│   │   └── Question.cs
│   └── Events/                          # Domain events published by this module
│       └── CategoryDeletedEvent.cs
│
├── Application/                         # CQRS, Business Logic
│   ├── Commands/
│   │   └── CreateQuestion/
│   │       ├── CreateQuestionCommand.cs
│   │       ├── CreateQuestionCommandHandler.cs
│   │       └── CreateQuestionCommandValidator.cs
│   ├── Queries/
│   │   └── GetQuestions/
│   │       ├── GetQuestionsQuery.cs
│   │       └── GetQuestionsQueryHandler.cs
│   ├── DTOs/
│   │   └── QuestionDto.cs
│   ├── Interfaces/
│   │   └── IQuestionRepository.cs
│   └── EventHandlers/                   # Event handlers (own + cross-module)
│       └── CategoryDeletedEventHandler.cs
│
├── Infrastructure/                      # Repositories, External Services
│   ├── Repositories/
│   │   └── QuestionRepository.cs
│   └── Services/
│       └── QuestionService.cs
│
└── <ModuleName>ModuleExtensions.cs      # DI registration for this module
```

**Key Principles:**
- ✅ **Vertical Slice:** All code for a business capability lives together
- ✅ **Module Autonomy:** Each module has its own Domain, Application, Infrastructure
- ✅ **No Direct References:** Modules never reference each other directly
- ✅ **Event-Driven Communication:** Use domain events for cross-module integration
- ✅ **Self-Registration:** Each module registers its own dependencies

---

## 2. Module Extension Template (NEW)

Each module provides a static extension method for dependency injection registration.

```csharp
// src/Modules/QuestionRandomizer.Modules.Questions/QuestionsModuleExtensions.cs
namespace QuestionRandomizer.Modules.Questions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using QuestionRandomizer.Modules.Questions.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Infrastructure.Repositories;

/// <summary>
/// Extension methods for registering Questions module services
/// </summary>
public static class QuestionsModuleExtensions
{
    /// <summary>
    /// Registers all services for the Questions module
    /// </summary>
    public static IServiceCollection AddQuestionsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register repositories (Infrastructure layer)
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IQualificationRepository, QualificationRepository>();

        // Register MediatR handlers (auto-discovery from this assembly)
        // This registers all IRequestHandler, INotificationHandler implementations
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register FluentValidation validators (auto-discovery)
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Optional: Register module-specific services
        // services.AddScoped<IQuestionService, QuestionService>();

        return services;
    }
}
```

**Usage in Program.cs:**
```csharp
using QuestionRandomizer.Modules.Questions;

// Add Questions module
builder.Services.AddQuestionsModule(builder.Configuration);
```

**Key Points:**
- One extension method per module
- Auto-discovery of handlers and validators
- Module encapsulates all its dependencies
- No references to other modules (except SharedKernel)

---

## 3. Domain Event Template (NEW)

Domain events enable decoupled communication between modules.

```csharp
// src/Modules/QuestionRandomizer.Modules.Questions/Domain/Events/CategoryDeletedEvent.cs
namespace QuestionRandomizer.Modules.Questions.Domain.Events;

using QuestionRandomizer.SharedKernel.Domain;

/// <summary>
/// Domain event raised when a category is deleted
/// Subscribed by: Randomization module (to remove category from active sessions)
/// </summary>
public class CategoryDeletedEvent : DomainEvent
{
    public CategoryDeletedEvent(string categoryId, string userId)
    {
        CategoryId = categoryId;
        UserId = userId;
    }

    /// <summary>
    /// ID of the deleted category
    /// </summary>
    public string CategoryId { get; }

    /// <summary>
    /// ID of the user who owns the category
    /// </summary>
    public string UserId { get; }
}
```

**Base class in SharedKernel:**
```csharp
// src/QuestionRandomizer.SharedKernel/Domain/DomainEvent.cs
namespace QuestionRandomizer.SharedKernel.Domain;

using MediatR;

/// <summary>
/// Base class for domain events (uses MediatR INotification)
/// </summary>
public abstract class DomainEvent : INotification
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

**Key Points:**
- Inherits from `DomainEvent` (which implements `INotification`)
- Immutable properties (init or readonly)
- Clear XML documentation indicating subscribers
- Published via MediatR

---

## 4. Event Handler Template (NEW)

Event handlers within the same module (self-cleanup).

```csharp
// src/Modules/QuestionRandomizer.Modules.Questions/Application/EventHandlers/CategoryDeletedEventHandler.cs
namespace QuestionRandomizer.Modules.Questions.Application.EventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Questions.Domain.Events;
using QuestionRandomizer.Modules.Questions.Application.Interfaces;

/// <summary>
/// Handles CategoryDeletedEvent within Questions module
/// Responsible for cleaning up questions that reference the deleted category
/// </summary>
public class CategoryDeletedEventHandler : INotificationHandler<CategoryDeletedEvent>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<CategoryDeletedEventHandler> _logger;

    public CategoryDeletedEventHandler(
        IQuestionRepository questionRepository,
        ILogger<CategoryDeletedEventHandler> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    public async Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CategoryDeleted event in Questions module for categoryId: {CategoryId}, userId: {UserId}",
            notification.CategoryId,
            notification.UserId);

        // Remove category reference from all questions
        var questions = await _questionRepository.GetByCategoryIdAsync(
            notification.CategoryId,
            notification.UserId,
            cancellationToken);

        foreach (var question in questions)
        {
            question.CategoryId = null;
            question.CategoryName = null;
            await _questionRepository.UpdateAsync(question, cancellationToken);
        }

        _logger.LogInformation(
            "Updated {Count} questions to remove category reference",
            questions.Count);
    }
}
```

**Key Points:**
- Implement `INotificationHandler<TEvent>`
- Multiple handlers can subscribe to the same event
- Use logging for observability
- Handle errors gracefully (don't throw unless critical)

---

## 5. Cross-Module Event Handler (NEW)

Event handlers in a different module (cross-module communication).

```csharp
// src/Modules/QuestionRandomizer.Modules.Randomization/Application/EventHandlers/CategoryDeletedEventHandler.cs
namespace QuestionRandomizer.Modules.Randomization.Application.EventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Questions.Domain.Events; // 🔥 References event from Questions module
using QuestionRandomizer.Modules.Randomization.Application.Interfaces;

/// <summary>
/// Handles CategoryDeletedEvent from Questions module
/// Responsible for removing the deleted category from all active randomization sessions
/// This demonstrates CROSS-MODULE COMMUNICATION via domain events
/// </summary>
public class CategoryDeletedEventHandler : INotificationHandler<CategoryDeletedEvent>
{
    private readonly IRandomizationRepository _randomizationRepository;
    private readonly ISelectedCategoryRepository _selectedCategoryRepository;
    private readonly ILogger<CategoryDeletedEventHandler> _logger;

    public CategoryDeletedEventHandler(
        IRandomizationRepository randomizationRepository,
        ISelectedCategoryRepository selectedCategoryRepository,
        ILogger<CategoryDeletedEventHandler> logger)
    {
        _randomizationRepository = randomizationRepository;
        _selectedCategoryRepository = selectedCategoryRepository;
        _logger = logger;
    }

    public async Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CategoryDeleted event in Randomization module for categoryId: {CategoryId}, userId: {UserId}",
            notification.CategoryId,
            notification.UserId);

        // Get all active randomization sessions for this user
        var activeRandomizations = await _randomizationRepository
            .GetActiveByUserIdAsync(notification.UserId, cancellationToken);

        // Remove the deleted category from all active sessions
        foreach (var randomization in activeRandomizations)
        {
            await _selectedCategoryRepository.DeleteAsync(
                randomization.Id,
                notification.CategoryId,
                cancellationToken);

            _logger.LogInformation(
                "Removed category {CategoryId} from randomization session {RandomizationId}",
                notification.CategoryId,
                randomization.Id);
        }
    }
}
```

**Key Points:**
- ✅ **Decoupled:** Randomization module reacts to Questions module events
- ✅ **No Direct Reference:** Only references the event class (not the entire module)
- ✅ **Autonomous:** Each module handles the event independently
- ✅ **Scalable:** Easy to add more subscribers without modifying the publisher

**Publishing Domain Events:**
```csharp
// In DeleteCategoryCommandHandler (Questions module)
public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
{
    var userId = _currentUserService.GetUserId();

    // Delete the category
    await _categoryRepository.DeleteAsync(request.Id, userId, cancellationToken);

    // Publish domain event (MediatR will notify all subscribers)
    await _mediator.Publish(
        new CategoryDeletedEvent(request.Id, userId),
        cancellationToken);
}
```

---

## 6. Domain Entity Template

```csharp
// src/QuestionRandomizer.Domain/Entities/Question.cs
namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents an interview question created by a user
/// </summary>
public class Question
{
    /// <summary>
    /// Unique identifier for the question (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The question text in English
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// The answer text in English
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// The answer text in Polish
    /// </summary>
    public string AnswerPl { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the category document ID (nullable)
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Denormalized category name for quick access
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Reference to the qualification document ID (nullable)
    /// </summary>
    public string? QualificationId { get; set; }

    /// <summary>
    /// Denormalized qualification name for quick access
    /// </summary>
    public string? QualificationName { get; set; }

    /// <summary>
    /// Whether the question is active (not deleted)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ID of the user who owns this question
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Optional tags for categorization
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// When the question was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the question was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
```

**Key Points:**
- POCOs with XML documentation
- Zero external dependencies
- Nullable reference types where appropriate
- DateTime properties for Firestore timestamps

---

## 2. Repository Interface Template

```csharp
// src/QuestionRandomizer.Domain/Interfaces/IQuestionRepository.cs
namespace QuestionRandomizer.Domain.Interfaces;

using QuestionRandomizer.Domain.Entities;

/// <summary>
/// Repository contract for Question data access
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// Get all questions for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of questions</returns>
    Task<List<Question>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a question by its ID
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Question if found and authorized, null otherwise</returns>
    Task<Question?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new question
    /// </summary>
    /// <param name="question">Question to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created question with ID</returns>
    Task<Question> CreateAsync(Question question, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing question
    /// </summary>
    /// <param name="question">Question with updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateAsync(Question question, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a question (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get questions by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of questions in the category</returns>
    Task<List<Question>> GetByCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default);
}
```

**Key Points:**
- Async methods with CancellationToken support
- Authorization via userId parameter
- Clear return types (null for not found)
- XML documentation for all methods

---

## 3. CQRS Command Template

```csharp
// src/QuestionRandomizer.Application/Commands/Questions/CreateQuestion/CreateQuestionCommand.cs
namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create a new question
/// </summary>
public record CreateQuestionCommand : IRequest<QuestionDto>
{
    public string QuestionText { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public string AnswerPl { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? QualificationId { get; init; }
    public List<string>? Tags { get; init; }
}
```

**Key Points:**
- Use `record` for immutability
- Implement `IRequest<TResponse>` from MediatR
- Properties with `init` for immutable setters
- Return DTOs, not domain entities

---

## 4. Command Handler Template

```csharp
// src/QuestionRandomizer.Application/Commands/Questions/CreateQuestion/CreateQuestionCommandHandler.cs
namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, QuestionDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QuestionDto> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Get category name if categoryId provided
        string? categoryName = null;
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, userId, cancellationToken);
            categoryName = category?.Name;
        }

        var question = new Question
        {
            QuestionText = request.QuestionText,
            Answer = request.Answer,
            AnswerPl = request.AnswerPl,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            QualificationId = request.QualificationId,
            Tags = request.Tags,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdQuestion = await _questionRepository.CreateAsync(question, cancellationToken);

        return new QuestionDto
        {
            Id = createdQuestion.Id,
            QuestionText = createdQuestion.QuestionText,
            Answer = createdQuestion.Answer,
            AnswerPl = createdQuestion.AnswerPl,
            CategoryId = createdQuestion.CategoryId,
            CategoryName = createdQuestion.CategoryName,
            QualificationId = createdQuestion.QualificationId,
            QualificationName = createdQuestion.QualificationName,
            IsActive = createdQuestion.IsActive,
            Tags = createdQuestion.Tags
        };
    }
}
```

**Key Points:**
- Implement `IRequestHandler<TRequest, TResponse>`
- Constructor injection for dependencies
- Get userId from ICurrentUserService
- Map domain entity to DTO before returning

---

## 5. FluentValidation Validator Template

```csharp
// src/QuestionRandomizer.Application/Commands/Questions/CreateQuestion/CreateQuestionCommandValidator.cs
namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

using FluentValidation;

/// <summary>
/// Validator for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required")
            .MaximumLength(5000).WithMessage("Answer must not exceed 5000 characters");

        RuleFor(x => x.AnswerPl)
            .NotEmpty().WithMessage("Polish answer is required")
            .MaximumLength(5000).WithMessage("Polish answer must not exceed 5000 characters");

        RuleFor(x => x.CategoryId)
            .MaximumLength(100).WithMessage("Category ID must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CategoryId));

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Maximum 20 tags allowed")
            .When(x => x.Tags != null);
    }
}
```

**Key Points:**
- Inherit from `AbstractValidator<T>`
- Chain validation rules fluently
- Use `.When()` for conditional validation
- Custom error messages with `.WithMessage()`

---

## 6. CQRS Query Template

```csharp
// src/QuestionRandomizer.Application/Queries/Questions/GetQuestions/GetQuestionsQuery.cs
namespace QuestionRandomizer.Application.Queries.Questions.GetQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all questions for the current user
/// </summary>
public record GetQuestionsQuery : IRequest<List<QuestionDto>>
{
    public string? CategoryId { get; init; }
    public bool? IsActive { get; init; }
}
```

**Key Points:**
- Use `record` for immutability
- Implement `IRequest<TResponse>`
- Optional filter properties

---

## 7. Query Handler Template

```csharp
// src/QuestionRandomizer.Application/Queries/Questions/GetQuestions/GetQuestionsQueryHandler.cs
namespace QuestionRandomizer.Application.Queries.Questions.GetQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for GetQuestionsQuery
/// </summary>
public class GetQuestionsQueryHandler : IRequestHandler<GetQuestionsQuery, List<QuestionDto>>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQuestionsQueryHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<QuestionDto>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var questions = string.IsNullOrEmpty(request.CategoryId)
            ? await _questionRepository.GetByUserIdAsync(userId, cancellationToken)
            : await _questionRepository.GetByCategoryIdAsync(request.CategoryId, userId, cancellationToken);

        if (request.IsActive.HasValue)
        {
            questions = questions.Where(q => q.IsActive == request.IsActive.Value).ToList();
        }

        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            Answer = q.Answer,
            AnswerPl = q.AnswerPl,
            CategoryId = q.CategoryId,
            CategoryName = q.CategoryName,
            QualificationId = q.QualificationId,
            QualificationName = q.QualificationName,
            IsActive = q.IsActive,
            Tags = q.Tags
        }).ToList();
    }
}
```

**Key Points:**
- Read operations (no side effects)
- Filter and map data
- Return DTOs

---

## 8. Controller Template

```csharp
// src/QuestionRandomizer.Api.Controllers/Controllers/QuestionsController.cs
namespace QuestionRandomizer.Api.Controllers.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Application.Commands.Questions.UpdateQuestion;
using QuestionRandomizer.Application.Commands.Questions.DeleteQuestion;
using QuestionRandomizer.Application.Queries.Questions.GetQuestions;
using QuestionRandomizer.Application.Queries.Questions.GetQuestionById;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Manages interview questions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all questions for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery { CategoryId = categoryId, IsActive = isActive };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific question by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionById(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new question
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuestion(
        [FromBody] CreateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuestionById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing question
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(
        string id,
        [FromBody] UpdateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in request body");

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a question (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(string id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteQuestionCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
```

**Key Points:**
- `[ApiController]` attribute for automatic model validation
- `[Authorize]` for authentication requirement
- MediatR for CQRS pattern
- `ProducesResponseType` for Swagger documentation
- Return `IActionResult` for flexibility

---

## 9. Minimal API Endpoint Template

```csharp
// src/QuestionRandomizer.Api.MinimalApi/Endpoints/QuestionEndpoints.cs
namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Application.Commands.Questions.UpdateQuestion;
using QuestionRandomizer.Application.Commands.Questions.DeleteQuestion;
using QuestionRandomizer.Application.Queries.Questions.GetQuestions;
using QuestionRandomizer.Application.Queries.Questions.GetQuestionById;
using QuestionRandomizer.Application.DTOs;

public static class QuestionEndpoints
{
    public static void MapQuestionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/questions")
            .RequireAuthorization()
            .WithTags("Questions");

        group.MapGet("", GetQuestions)
            .WithName("GetQuestions")
            .Produces<List<QuestionDto>>(StatusCodes.Status200OK);

        group.MapGet("{id}", GetQuestionById)
            .WithName("GetQuestionById")
            .Produces<QuestionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateQuestion)
            .WithName("CreateQuestion")
            .Produces<QuestionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("{id}", UpdateQuestion)
            .WithName("UpdateQuestion")
            .Produces<QuestionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id}", DeleteQuestion)
            .WithName("DeleteQuestion")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<List<QuestionDto>>> GetQuestions(
        IMediator mediator,
        string? categoryId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery { CategoryId = categoryId, IsActive = isActive };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<QuestionDto>, NotFound>> GetQuestionById(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionByIdQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Created<QuestionDto>> CreateQuestion(
        CreateQuestionCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/questions/{result.Id}", result);
    }

    private static async Task<Results<Ok<QuestionDto>, NotFound>> UpdateQuestion(
        string id,
        UpdateQuestionCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
            return TypedResults.NotFound();

        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteQuestion(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteQuestionCommand { Id = id };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
```

**Key Points:**
- Static extension method for registration
- `MapGroup()` for route prefixing
- `RequireAuthorization()` for authentication
- `TypedResults` for strongly-typed responses
- Parameter injection (IMediator injected automatically)

---

## 10. Repository Implementation Template

```csharp
// src/QuestionRandomizer.Infrastructure/Repositories/QuestionRepository.cs
namespace QuestionRandomizer.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Firestore implementation of IQuestionRepository
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "questions";

    public QuestionRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Question>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("userId", userId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<Question>())
            .ToList();
    }

    public async Task<Question?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
            return null;

        var question = snapshot.ConvertTo<Question>();

        // Verify ownership
        if (question.UserId != userId)
            return null;

        return question;
    }

    public async Task<Question> CreateAsync(Question question, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(CollectionName).AddAsync(question, cancellationToken);
        question.Id = docRef.Id;
        return question;
    }

    public async Task<bool> UpdateAsync(Question question, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(question.Id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
            return false;

        var existingQuestion = snapshot.ConvertTo<Question>();
        if (existingQuestion.UserId != question.UserId)
            return false;

        question.UpdatedAt = DateTime.UtcNow;
        await docRef.SetAsync(question, SetOptions.Overwrite, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
            return false;

        var question = snapshot.ConvertTo<Question>();
        if (question.UserId != userId)
            return false;

        // Soft delete by setting IsActive = false
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "isActive", false },
            { "updatedAt", FieldValue.ServerTimestamp }
        }, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<List<Question>> GetByCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("userId", userId)
            .WhereEqualTo("categoryId", categoryId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<Question>())
            .ToList();
    }
}
```

**Key Points:**
- Inject `FirestoreDb` via constructor
- Use `ConvertTo<T>()` for deserialization
- Always verify ownership before operations
- Soft delete pattern (set IsActive = false)

---

## 16. Program.cs Template (Modular)

```csharp
// src/QuestionRandomizer.Api.Controllers/Program.cs
using QuestionRandomizer.SharedKernel;
using QuestionRandomizer.Modules.Questions;
using QuestionRandomizer.Modules.Conversations;
using QuestionRandomizer.Modules.Randomization;
using QuestionRandomizer.Modules.Agent;
using Hellang.Middleware.ProblemDetails;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
});

// Add SharedKernel (cross-cutting concerns, domain events infrastructure)
builder.Services.AddSharedKernel(builder.Configuration, builder.Environment);

// Add Modules (modular monolith architecture)
builder.Services.AddQuestionsModule(builder.Configuration);
builder.Services.AddConversationsModule(builder.Configuration);
builder.Services.AddRandomizationModule();
builder.Services.AddAgentModule(builder.Configuration);

// LEGACY - Old Clean Architecture layers (to be removed later)
// builder.Services.AddApplication();
// builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("QuestionRandomizer.Api.Controllers"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure pipeline
app.UseProblemDetails();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible to tests
public partial class Program { }
```

**Key Points:**
- ✅ **Modular Registration:** Each module registers its own dependencies
- ✅ **SharedKernel First:** Cross-cutting concerns and domain events infrastructure
- ✅ **Clean Separation:** Each module is self-contained
- ✅ **No Module References:** Modules don't know about each other (only SharedKernel)
- ProblemDetails for consistent error handling
- OpenTelemetry for observability
- Health checks endpoint

**Module Registration Order:**
1. SharedKernel (must be first - provides domain events infrastructure)
2. Business Modules (Questions, Conversations, Randomization, Agent)
3. Infrastructure concerns (CORS, OpenTelemetry, etc.)

---

## 12. Validation Pipeline Behavior

```csharp
// src/QuestionRandomizer.Application/Behaviors/ValidationBehavior.cs
namespace QuestionRandomizer.Application.Behaviors;

using FluentValidation;
using MediatR;

/// <summary>
/// MediatR pipeline behavior that validates commands/queries using FluentValidation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

**Key Points:**
- Implement `IPipelineBehavior<TRequest, TResponse>`
- Run all validators before handler execution
- Throw ValidationException if validation fails
- Register in DI: `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))`

---

## 13. Unit Test Template

```csharp
// tests/QuestionRandomizer.UnitTests/Application/Commands/CreateQuestionCommandHandlerTests.cs
namespace QuestionRandomizer.UnitTests.Application.Commands;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;
using Xunit;

public class CreateQuestionCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateQuestionCommandHandler _handler;

    public CreateQuestionCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateQuestionCommandHandler(
            _questionRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesQuestion()
    {
        // Arrange
        const string userId = "user123";
        const string categoryId = "cat456";
        const string categoryName = "JavaScript";

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, Name = categoryName, UserId = userId });

        _questionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question q, CancellationToken ct) =>
            {
                q.Id = "q789";
                return q;
            });

        var command = new CreateQuestionCommand
        {
            QuestionText = "What is a closure?",
            Answer = "A closure is...",
            AnswerPl = "Domknięcie to...",
            CategoryId = categoryId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("q789");
        result.QuestionText.Should().Be(command.QuestionText);
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(categoryName);

        _questionRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<Question>(q =>
                q.QuestionText == command.QuestionText &&
                q.UserId == userId &&
                q.CategoryId == categoryId &&
                q.CategoryName == categoryName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoCategoryId_CreatesQuestionWithoutCategory()
    {
        // Arrange
        const string userId = "user123";

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question q, CancellationToken ct) =>
            {
                q.Id = "q789";
                return q;
            });

        var command = new CreateQuestionCommand
        {
            QuestionText = "What is a closure?",
            Answer = "A closure is...",
            AnswerPl = "Domknięcie to..."
        };

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
}
```

**Key Points:**
- Arrange-Act-Assert pattern
- Mock dependencies with Moq
- FluentAssertions for readable assertions
- Verify method calls with `Verify()`
- Test both happy path and edge cases

---

## Summary

These templates provide the foundation for the Question Randomizer Backend project. Follow these patterns consistently to maintain code quality and architectural integrity.

**Key Principles:**

### Architecture
- ✅ **Modular Monolith** (vertical slices by business capability)
- ✅ **Module Autonomy** (each module: Domain → Application → Infrastructure)
- ✅ **Event-Driven Communication** (domain events for cross-module integration)
- ✅ **SharedKernel** (cross-cutting concerns, domain events infrastructure)

### Patterns
- ✅ **CQRS with MediatR** (Commands, Queries, Domain Events)
- ✅ **Repository Pattern** (abstraction over Firestore)
- ✅ **Dependency Injection** (module self-registration)
- ✅ **FluentValidation** (input validation pipeline)

### Coding Standards
- ✅ **Async/await throughout** (all I/O operations)
- ✅ **XML documentation** (all public types and members)
- ✅ **Comprehensive testing** (unit, integration, E2E)
- ✅ **Immutability** (records for commands/queries/events)

### Module Guidelines
- ✅ **No Direct Module References** (only via domain events)
- ✅ **Clear Boundaries** (module folders enforce separation)
- ✅ **Self-Contained** (module includes all layers)
- ✅ **Observable** (logging in event handlers)

**For complete migration details, architecture comparison, and module statistics, see [MIGRATION-SUMMARY.md](../MIGRATION-SUMMARY.md).**
