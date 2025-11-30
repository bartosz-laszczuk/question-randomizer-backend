# Testing Strategy

Comprehensive testing guide for Question Randomizer Backend project.

---

## Overview

Testing is organized into three layers:
1. **Unit Tests** - Fast, isolated tests for business logic
2. **Integration Tests** - Test API endpoints with real dependencies
3. **E2E Tests** - Test complete user workflows

**Coverage Goals:**
- **Minimum:** 70% overall
- **Target:** 80% overall
- **Critical paths:** 95% (authentication, CRUD operations)

**Current Status (2025-11-30):**
- ✅ **170 unit tests passing** (26 handler test files)
- ✅ **54.6% line coverage** (834/1527 lines covered)
- ✅ **81.6% branch coverage** (62/76 branches covered)
- ✅ **~230ms execution time** for all unit tests
- ✅ **50 integration tests passing** (100% pass rate - Controllers API)
- ⏳ E2E tests pending

---

## Test Projects

```
tests/
├── QuestionRandomizer.UnitTests/              # Unit tests (handlers, validators)
├── QuestionRandomizer.IntegrationTests.Controllers/  # Integration tests (Controllers API)
└── QuestionRandomizer.E2ETests/               # End-to-end tests (full system)
```

---

## 1. Unit Tests

### Purpose
Test business logic in isolation without external dependencies.

### What to Test
- ✅ Command/Query handlers
- ✅ FluentValidation validators
- ✅ Domain logic
- ✅ Business rules
- ✅ Mapping logic

### Tools
- **xUnit** - Test framework
- **Moq** - Mocking dependencies
- **FluentAssertions** - Readable assertions
- **Bogus** - Fake data generation

### Implemented Test Structure (170 Tests)

```
tests/QuestionRandomizer.UnitTests/
├── Commands/
│   ├── Questions/
│   │   ├── CreateQuestionCommandHandlerTests.cs (6 tests)
│   │   ├── UpdateQuestionCommandHandlerTests.cs (10 tests)
│   │   ├── DeleteQuestionCommandHandlerTests.cs (6 tests)
│   │   ├── CreateQuestionCommandValidatorTests.cs (5 tests)
│   │   └── UpdateQuestionCommandValidatorTests.cs (21 tests)
│   ├── Categories/
│   │   ├── CreateCategoryCommandHandlerTests.cs (5 tests)
│   │   ├── UpdateCategoryCommandHandlerTests.cs (4 tests)
│   │   └── DeleteCategoryCommandHandlerTests.cs (3 tests)
│   ├── Qualifications/
│   │   ├── CreateQualificationCommandHandlerTests.cs (5 tests)
│   │   ├── UpdateQualificationCommandHandlerTests.cs (4 tests)
│   │   └── DeleteQualificationCommandHandlerTests.cs (3 tests)
│   ├── Conversations/
│   │   ├── CreateConversationCommandHandlerTests.cs (3 tests)
│   │   ├── DeleteConversationCommandHandlerTests.cs (3 tests)
│   │   └── UpdateConversationTimestampCommandHandlerTests.cs (4 tests)
│   ├── Messages/
│   │   └── AddMessageCommandHandlerTests.cs (4 tests)
│   └── Randomizations/
│       ├── CreateRandomizationCommandHandlerTests.cs (5 tests)
│       ├── UpdateRandomizationCommandHandlerTests.cs (6 tests)
│       └── ClearCurrentQuestionCommandHandlerTests.cs (4 tests)
└── Queries/
    ├── Questions/
    │   ├── GetQuestionsQueryHandlerTests.cs (39 tests)
    │   └── GetQuestionByIdQueryHandlerTests.cs (6 tests)
    ├── Categories/
    │   ├── GetCategoriesQueryHandlerTests.cs (6 tests)
    │   └── GetCategoryByIdQueryHandlerTests.cs (5 tests)
    ├── Qualifications/
    │   ├── GetQualificationsQueryHandlerTests.cs (6 tests)
    │   └── GetQualificationByIdQueryHandlerTests.cs (5 tests)
    ├── Conversations/
    │   ├── GetConversationsQueryHandlerTests.cs (3 tests)
    │   └── GetConversationByIdQueryHandlerTests.cs (4 tests)
    ├── Messages/
    │   └── GetMessagesQueryHandlerTests.cs (5 tests)
    └── Randomizations/
        └── GetRandomizationQueryHandlerTests.cs (5 tests)

Total: 170 tests across 26 test files
Coverage: 54.6% line, 81.6% branch
Execution: ~230ms
```

**Covered Handlers (26/43):**
- ✅ Questions: Complete CRUD + validators
- ✅ Categories: Complete CRUD
- ✅ Qualifications: Complete CRUD
- ✅ Conversations: Create, Read, Update, Delete, UpdateTimestamp
- ✅ Messages: Add, Get
- ✅ Randomizations: Create, Update, Clear, Get

**Pending Handlers (17/43):**
- ⏳ PostponedQuestions (4 handlers)
- ⏳ SelectedCategories (3 handlers)
- ⏳ UsedQuestions (4 handlers)
- ⏳ Batch Operations (3 handlers)
- ⏳ Remove Operations (2 handlers)
- ⏳ UpdateQuestionsBatch (1 handler)

### Example: Testing a Command Handler

```csharp
// tests/QuestionRandomizer.UnitTests/Application/Commands/CreateQuestionCommandHandlerTests.cs
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
        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
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
        result.Id.Should().Be("q789");
        result.QuestionText.Should().Be(command.QuestionText);

        _questionRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<Question>(q => q.QuestionText == command.QuestionText),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCategory_AssignsCategoryName()
    {
        // Arrange
        const string userId = "user123";
        const string categoryId = "cat456";
        const string categoryName = "JavaScript";

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, Name = categoryName });

        _questionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
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
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(categoryName);
    }
}
```

### Example: Testing a Validator

```csharp
// tests/QuestionRandomizer.UnitTests/Application/Validators/CreateQuestionCommandValidatorTests.cs
public class CreateQuestionCommandValidatorTests
{
    private readonly CreateQuestionCommandValidator _validator;

    public CreateQuestionCommandValidatorTests()
    {
        _validator = new CreateQuestionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "What is a closure?",
            Answer = "A closure is...",
            AnswerPl = "Domknięcie to..."
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyQuestionText_ReturnsError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "",
            Answer = "A closure is...",
            AnswerPl = "Domknięcie to..."
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateQuestionCommand.QuestionText));
    }

    [Theory]
    [InlineData(1001)] // Too long
    [InlineData(2000)] // Way too long
    public void Validate_QuestionTextTooLong_ReturnsError(int length)
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = new string('x', length),
            Answer = "A closure is...",
            AnswerPl = "Domknięcie to..."
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("1000 characters"));
    }
}
```

### Running Unit Tests

```bash
# Run all unit tests
dotnet test tests/QuestionRandomizer.UnitTests

# Run all unit tests with quiet output
dotnet test tests/QuestionRandomizer.UnitTests --verbosity quiet

# Run specific test class
dotnet test tests/QuestionRandomizer.UnitTests --filter "FullyQualifiedName~CreateQuestionCommandHandlerTests"

# Run specific test method
dotnet test tests/QuestionRandomizer.UnitTests --filter "FullyQualifiedName~Handle_ValidCommand_CreatesQuestion"

# Run with detailed output
dotnet test tests/QuestionRandomizer.UnitTests --logger "console;verbosity=detailed"

# Run with code coverage (XPlat Code Coverage - recommended)
dotnet test tests/QuestionRandomizer.UnitTests --collect:"XPlat Code Coverage" --results-directory:./coverage

# View coverage report (generated in coverage/*/coverage.cobertura.xml)
# Coverage summary shows: line-rate, branch-rate, lines-covered, lines-valid
```

**Current Results (2025-01-28):**
```
Success!  - Failed:     0, Passed:   170, Skipped:     0, Total:   170, Duration: 230 ms
Line Coverage:   54.6% (834/1527 lines)
Branch Coverage: 81.6% (62/76 branches)
```

---

## 2. Integration Tests

### Purpose
Test API endpoints with real HTTP requests and mocked dependencies.

### What to Test
- ✅ API endpoint functionality
- ✅ Request/response serialization
- ✅ Authentication/authorization
- ✅ Middleware pipeline
- ✅ HTTP status codes
- ✅ CRUD operations
- ✅ Validation error handling

### Tools
- **WebApplicationFactory** - In-memory test server
- **Moq** - Mocked repositories
- **FluentAssertions** - Readable assertions
- **xUnit** - Test framework with IClassFixture

### Implemented Test Structure (50 Tests - 100% Pass Rate)

**Status:** ✅ **All 50/50 integration tests passing**

```
tests/QuestionRandomizer.IntegrationTests.Controllers/
├── Infrastructure/
│   ├── CustomWebApplicationFactory.cs       # Test infrastructure
│   └── TestAuthHandler.cs                   # Auto-authentication handler
└── Controllers/
    ├── QuestionsControllerTests.cs          # 12 tests ✅
    ├── CategoriesControllerTests.cs         # 10 tests ✅
    ├── QualificationsControllerTests.cs     # 10 tests ✅
    ├── ConversationsControllerTests.cs      # 11 tests ✅
    └── RandomizationsControllerTests.cs     #  8 tests ✅
```

**Test Coverage:**
- **QuestionsController** (12 tests) - GET, GET by ID, POST, PUT, DELETE, filters, validation
- **CategoriesController** (10 tests) - GET, GET by ID, POST, POST batch, PUT, DELETE, validation
- **QualificationsController** (10 tests) - GET, GET by ID, POST, POST batch, PUT, DELETE, validation
- **ConversationsController** (11 tests) - GET, GET by ID, POST, DELETE, messages, timestamp updates
- **RandomizationsController** (8 tests) - GET, POST, PUT, Clear, validation, ID mismatch

**Scenarios Covered:**
- ✅ Success paths (200 OK, 201 Created, 204 NoContent)
- ✅ NotFound scenarios (404)
- ✅ BadRequest validation (400)
- ✅ Batch operations
- ✅ ID mismatch validation
- ✅ Empty results
- ✅ Authentication (all requests auto-authenticated)

### Example: Integration Test Pattern

```csharp
// tests/QuestionRandomizer.IntegrationTests.Controllers/Controllers/QuestionsControllerTests.cs
public class QuestionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QuestionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.ResetMocks(); // Reset mocks before each test
    }

    [Fact]
    public async Task GetQuestions_ReturnsOkWithQuestions()
    {
        // Arrange
        var questions = new List<Question>
        {
            new() { Id = "q1", QuestionText = "Test?", UserId = CustomWebApplicationFactory.TestUserId }
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var response = await _client.GetAsync("/api/questions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<QuestionDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateQuestion_InvalidCommand_ReturnsBadRequest()
    {
        // Arrange - empty QuestionText triggers validation error
        var command = new CreateQuestionCommand
        {
            QuestionText = "",
            Answer = "Answer",
            AnswerPl = "Odpowiedź"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/questions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

### Custom WebApplicationFactory

```csharp
// tests/QuestionRandomizer.IntegrationTests.Controllers/Infrastructure/CustomWebApplicationFactory.cs
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IQuestionRepository> QuestionRepositoryMock { get; } = new();
    public Mock<ICategoryRepository> CategoryRepositoryMock { get; } = new();
    // ... other repository mocks

    public const string TestUserId = "test-user-123";
    public const string TestUserEmail = "test@example.com";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real repositories
            RemoveService<IQuestionRepository>(services);
            // ... remove other services

            // Remove Swagger to prevent assembly loading issues
            var partManager = services
                .FirstOrDefault(d => d.ServiceType == typeof(ApplicationPartManager))
                ?.ImplementationInstance as ApplicationPartManager;
            if (partManager != null)
            {
                var swaggerParts = partManager.ApplicationParts
                    .Where(p => p.Name.Contains("Swashbuckle") || p.Name.Contains("OpenApi"))
                    .ToList();
                foreach (var part in swaggerParts)
                {
                    partManager.ApplicationParts.Remove(part);
                }
            }

            // Setup test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            // Register mocked repositories
            services.AddSingleton(QuestionRepositoryMock.Object);
            // ... register other mocks
        });
    }

    public void ResetMocks()
    {
        QuestionRepositoryMock.Reset();
        // ... reset other mocks
    }
}
```

### Test Authentication Handler

```csharp
// tests/QuestionRandomizer.IntegrationTests.Controllers/Infrastructure/TestAuthHandler.cs
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, CustomWebApplicationFactory.TestUserId),
            new Claim(ClaimTypes.Email, CustomWebApplicationFactory.TestUserEmail),
            new Claim(ClaimTypes.Name, "Test User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

### Running Integration Tests

```bash
# Run all integration tests
dotnet test tests/QuestionRandomizer.IntegrationTests.Controllers

# Run specific controller tests
dotnet test tests/QuestionRandomizer.IntegrationTests.Controllers --filter "FullyQualifiedName~QuestionsControllerTests"

# Run with detailed output
dotnet test tests/QuestionRandomizer.IntegrationTests.Controllers --logger "console;verbosity=detailed"
```

**Current Results (2025-11-30):**
```
Success!  - Failed: 0, Passed: 50, Skipped: 0, Total: 50
Pass Rate: 100%
```

### Key Issues Resolved

1. **OpenAPI Dependency Conflict** - Excluded Swagger assemblies from test ApplicationPartManager
2. **Authentication** - Created TestAuthHandler for automatic authentication
3. **FluentValidation Error Mapping** - Fixed ValidationBehavior to throw custom ValidationException
4. **Command Validation** - Ensured update commands include Id property
5. **ASP.NET Core Route Casing** - Updated assertions to match framework behavior
6. **Randomization Status Values** - Aligned test data with validator requirements ("Ongoing"/"Completed")

**📖 See [INTEGRATION-TEST-SUMMARY.md](../INTEGRATION-TEST-SUMMARY.md) for detailed breakdown of all issues and resolutions.**

---

## 3. End-to-End Tests

### Purpose
Test complete user workflows from start to finish.

### What to Test
- ✅ User authentication flow
- ✅ Complete CRUD workflows
- ✅ Multi-step scenarios
- ✅ Error handling
- ✅ Business workflows

### Example: E2E Test Scenario

```csharp
// tests/QuestionRandomizer.E2ETests/Scenarios/QuestionManagementE2ETests.cs
public class QuestionManagementE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public QuestionManagementE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task CompleteQuestionLifecycle_CreateUpdateDelete_Success()
    {
        // 1. Create question
        var createCommand = new CreateQuestionCommand
        {
            QuestionText = "What is DDD?",
            Answer = "Domain-Driven Design...",
            AnswerPl = "Projektowanie sterowane domeną..."
        };

        var createResponse = await _client.PostAsJsonAsync("/api/questions", createCommand);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<QuestionDto>();
        created.Should().NotBeNull();
        var questionId = created!.Id;

        // 2. Get question by ID
        var getResponse = await _client.GetAsync($"/api/questions/{questionId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<QuestionDto>();
        retrieved.Should().NotBeNull();
        retrieved!.QuestionText.Should().Be("What is DDD?");

        // 3. Update question
        var updateCommand = new UpdateQuestionCommand
        {
            Id = questionId,
            QuestionText = "What is Domain-Driven Design?",
            Answer = "DDD is...",
            AnswerPl = "DDD to..."
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/questions/{questionId}", updateCommand);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Verify update
        var verifyResponse = await _client.GetAsync($"/api/questions/{questionId}");
        var updated = await verifyResponse.Content.ReadFromJsonAsync<QuestionDto>();
        updated!.QuestionText.Should().Be("What is Domain-Driven Design?");

        // 5. Delete question
        var deleteResponse = await _client.DeleteAsync($"/api/questions/{questionId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verify deletion (soft delete - should still exist but IsActive = false)
        var deletedResponse = await _client.GetAsync($"/api/questions/{questionId}");
        var deleted = await deletedResponse.Content.ReadFromJsonAsync<QuestionDto>();
        deleted!.IsActive.Should().BeFalse();
    }
}
```

### Running E2E Tests

```bash
# Run all E2E tests
dotnet test tests/QuestionRandomizer.E2ETests

# Run specific scenario
dotnet test tests/QuestionRandomizer.E2ETests --filter "FullyQualifiedName~CompleteQuestionLifecycle"
```

---

## Code Coverage

### Generate Coverage Report

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Install report generator tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage -reporttypes:Html

# Open report
start coverage/index.html  # Windows
open coverage/index.html   # macOS
```

### Coverage Thresholds

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <CoverletOutput>./coverage/</CoverletOutput>
  <CoverletOutputFormat>opencover</CoverletOutputFormat>
  <Threshold>80</Threshold>
  <ThresholdType>line,branch</ThresholdType>
  <ThresholdStat>total</ThresholdStat>
</PropertyGroup>
```

---

## Test Data Generation

### Using Bogus

```csharp
// tests/QuestionRandomizer.UnitTests/TestHelpers/FakeDataGenerator.cs
public static class FakeDataGenerator
{
    public static Faker<Question> QuestionFaker => new Faker<Question>()
        .RuleFor(q => q.Id, f => f.Random.Guid().ToString())
        .RuleFor(q => q.QuestionText, f => f.Lorem.Sentence())
        .RuleFor(q => q.Answer, f => f.Lorem.Paragraph())
        .RuleFor(q => q.AnswerPl, f => f.Lorem.Paragraph())
        .RuleFor(q => q.IsActive, f => true)
        .RuleFor(q => q.UserId, f => f.Random.Guid().ToString())
        .RuleFor(q => q.CreatedAt, f => f.Date.Recent())
        .RuleFor(q => q.UpdatedAt, f => f.Date.Recent());

    public static Question GenerateQuestion() => QuestionFaker.Generate();

    public static List<Question> GenerateQuestions(int count) => QuestionFaker.Generate(count);
}

// Usage
var fakeQuestions = FakeDataGenerator.GenerateQuestions(10);
```

---

## Best Practices

### ✅ DO:
- Use Arrange-Act-Assert pattern
- Test one thing per test
- Use descriptive test names (MethodName_Scenario_ExpectedBehavior)
- Mock external dependencies
- Use FluentAssertions for readability
- Run tests before committing
- Maintain >80% code coverage

### ❌ DON'T:
- Test framework code (ASP.NET Core internals)
- Test third-party libraries
- Write slow tests in unit test suite
- Share state between tests
- Use magic numbers/strings
- Skip edge cases

---

## Continuous Integration

### GitHub Actions Example

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Run tests
        run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.opencover.xml
```

---

## Quick Reference

```bash
# Run all tests
dotnet test

# Run specific project
dotnet test tests/QuestionRandomizer.UnitTests

# Run with filter
dotnet test --filter "FullyQualifiedName~CreateQuestion"

# Run with coverage
dotnet test tests/QuestionRandomizer.UnitTests --collect:"XPlat Code Coverage" --results-directory:./coverage

# Watch mode (auto-run on changes)
dotnet watch test --project tests/QuestionRandomizer.UnitTests
```

---

## Testing Roadmap

### ✅ Phase 6A: Core Unit Tests (Complete - 2025-01-28)
- **Status:** 170 tests passing, 54.6% line coverage, 81.6% branch coverage
- **Completed Modules:**
  - Questions (78 tests) - Complete CRUD + validators
  - Categories (23 tests) - Complete CRUD
  - Qualifications (23 tests) - Complete CRUD
  - Conversations (20 tests) - Complete CRUD + UpdateTimestamp
  - Messages (9 tests) - Add + Get
  - Randomizations (20 tests) - Create, Update, Clear, Get

### ⏳ Phase 6B: Remaining Unit Tests (Pending)
- **Target:** Add ~110 tests to reach 80%+ coverage
- **Priority Handlers:**
  1. Batch Operations (3 handlers) - CreateCategoriesBatch, CreateQualificationsBatch, CreateQuestionsBatch
  2. PostponedQuestions (4 handlers) - Add, Delete, Get, UpdateTimestamp
  3. UsedQuestions (4 handlers) - Add, Delete, Get, UpdateCategory
  4. SelectedCategories (3 handlers) - Add, Delete, Get
  5. Remove Operations (2 handlers) - RemoveCategoryFromQuestions, RemoveQualificationFromQuestions
  6. UpdateQuestionsBatch (1 handler)

### ✅ Phase 6C: Integration Tests - Controllers API (Complete - 2025-11-30)
- **Status:** 50/50 tests passing (100% pass rate)
- **Completed Infrastructure:**
  - ✅ CustomWebApplicationFactory configured
  - ✅ TestAuthHandler for automatic authentication
  - ✅ All repository mocks setup
  - ✅ Swagger assembly exclusion to prevent type loading issues
- **Completed Test Files:**
  - ✅ QuestionsControllerTests (12 tests)
  - ✅ CategoriesControllerTests (10 tests)
  - ✅ QualificationsControllerTests (10 tests)
  - ✅ ConversationsControllerTests (11 tests)
  - ✅ RandomizationsControllerTests (8 tests)
- **Issues Resolved:**
  - ✅ OpenAPI dependency conflict
  - ✅ Authentication configuration
  - ✅ FluentValidation error mapping
  - ✅ Command validation (Id property)
  - ✅ ASP.NET Core route casing
  - ✅ Randomization status values

### ⏳ Phase 6D: Integration Tests - Minimal API (Optional)
- **Target:** Mirror Controllers API tests for Minimal API (Port 5001)
- **Estimated:** ~50 tests (same coverage as Controllers)

### ⏳ Phase 6E: E2E Tests (Pending)
- **Target:** Test critical user workflows
- **Scenarios:**
  - User registration and authentication
  - Create and manage questions
  - Randomization workflow
  - Conversation management

**Estimated Timeline:**
- Phase 6B: ~2-3 hours (remaining unit tests)
- Phase 6D: ~2-3 hours (Minimal API integration tests - optional)
- Phase 6E: ~2-3 hours (E2E tests)
- **Total:** ~6-9 hours to complete all testing phases
