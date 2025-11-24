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

# Run specific test class
dotnet test tests/QuestionRandomizer.UnitTests --filter "FullyQualifiedName~CreateQuestionCommandHandlerTests"

# Run specific test method
dotnet test tests/QuestionRandomizer.UnitTests --filter "FullyQualifiedName~Handle_ValidCommand_CreatesQuestion"

# Run with detailed output
dotnet test tests/QuestionRandomizer.UnitTests --logger "console;verbosity=detailed"
```

---

## 2. Integration Tests

### Purpose
Test API endpoints with real HTTP requests and real dependencies (or test doubles).

### What to Test
- ✅ API endpoint functionality
- ✅ Request/response serialization
- ✅ Authentication/authorization
- ✅ Middleware pipeline
- ✅ Database operations (with test containers)

### Tools
- **WebApplicationFactory** - In-memory test server
- **TestContainers** - Docker containers for dependencies
- **FluentAssertions** - Readable assertions

### Example: Testing Controllers API

```csharp
// tests/QuestionRandomizer.IntegrationTests.Controllers/QuestionsControllerTests.cs
public class QuestionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuestionsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetQuestions_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/questions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetQuestions_WithAuth_ReturnsOk()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "fake-test-token");

        // Act
        var response = await _client.GetAsync("/api/questions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var questions = await response.Content.ReadFromJsonAsync<List<QuestionDto>>();
        questions.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateQuestion_ValidData_Returns201()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "fake-test-token");

        var command = new CreateQuestionCommand
        {
            QuestionText = "What is CQRS?",
            Answer = "CQRS is...",
            AnswerPl = "CQRS to..."
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/questions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<QuestionDto>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }
}
```

### Custom WebApplicationFactory

```csharp
// tests/QuestionRandomizer.IntegrationTests.Controllers/CustomWebApplicationFactory.cs
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace Firebase with mock
            services.RemoveAll<FirestoreDb>();
            services.AddSingleton<FirestoreDb>(sp => CreateMockFirestoreDb());

            // Replace authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });
    }

    private FirestoreDb CreateMockFirestoreDb()
    {
        // Create mock or use Firebase Emulator
        return Mock.Of<FirestoreDb>();
    }
}
```

### Running Integration Tests

```bash
# Run all integration tests
dotnet test tests/QuestionRandomizer.IntegrationTests.Controllers

# Run with Docker containers (TestContainers)
docker ps  # Ensure Docker is running
dotnet test tests/QuestionRandomizer.IntegrationTests.Controllers
```

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
dotnet test /p:CollectCoverage=true

# Watch mode (auto-run on changes)
dotnet watch test --project tests/QuestionRandomizer.UnitTests
```
