# End-to-End Tests

## Overview

This project contains End-to-End (E2E) tests that verify complete user workflows across multiple API endpoints. These tests ensure that the Question Randomizer Backend API functions correctly from a user's perspective.

## Test Structure

### Infrastructure

- **E2ETestWebApplicationFactory.cs** - Custom WebApplicationFactory that configures the test environment
  - Uses test authentication (bypasses Firebase Auth)
  - Injects test user credentials
  - Skips Firebase Firestore initialization (uses Testing environment)

- **TestAuthHandler.cs** - Authentication handler for tests
  - Automatically authenticates all requests
  - Uses predefined test user (e2e-test-user-123)

- **TestCurrentUserService.cs** - Mock user service
  - Returns test user ID and email

- **E2ETestBase.cs** - Base class for all E2E tests
  - Provides HTTP helper methods (GetAsync, PostAsync, PutAsync, DeleteAsync)
  - Assertion helpers (AssertStatusCode, AssertNotFound, etc.)
  - Test data helpers (CreateTestString, WaitAsync)

### Workflow Tests

#### 1. QuestionLifecycleE2ETests (6 tests)

Tests the complete lifecycle of questions from creation to deletion.

**Tests:**
1. `CompleteQuestionLifecycle_ShouldSucceed` - Full CRUD workflow
   - Create category and qualification
   - Create question with both
   - Get question by ID
   - Update question
   - Get all questions
   - Delete question
   - Verify 404 after deletion

2. `CreateQuestion_WithoutCategoryAndQualification_ShouldSucceed` - Minimal question creation
3. `UpdateQuestion_ChangingCategoryAndQualification_ShouldSucceed` - Update associations
4. `GetQuestion_WithInvalidId_ShouldReturn404` - Error handling
5. `DeleteQuestion_WithInvalidId_ShouldReturn404` - Error handling
6. `CreateQuestion_WithInvalidData_ShouldReturnBadRequest` - Validation

#### 2. RandomizationWorkflowE2ETests (8 tests)

Tests randomization sessions including question tracking (used/postponed).

**Tests:**
1. `CompleteRandomizationWorkflow_ShouldSucceed` - Full randomization workflow
   - Create category and 10 questions
   - Create randomization session
   - Mark question as used
   - Mark question as postponed
   - Get used/postponed question lists
   - Update randomization (show answer, current question)
   - Complete randomization

2. `RandomizationWorkflow_WithMultipleUsedQuestions_ShouldTrackAll` - Multiple used questions
3. `RandomizationWorkflow_RemoveUsedQuestion_ShouldSucceed` - Remove used question
4. `RandomizationWorkflow_RemovePostponedQuestion_ShouldSucceed` - Remove postponed question
5. `RandomizationWorkflow_ClearCurrentQuestion_ShouldSucceed` - Clear current question
6. `GetRandomization_WhenNoActiveSession_ShouldReturn404` - Error handling
7. `UpdateRandomization_WithInvalidId_ShouldReturn404` - Error handling
8. `AddUsedQuestion_WithInvalidRandomizationId_ShouldReturn404` - Error handling

#### 3. ConversationWorkflowE2ETests (10 tests)

Tests conversation and message management workflows.

**Tests:**
1. `CompleteConversationWorkflow_ShouldSucceed` - Full conversation workflow
   - Create conversation with title
   - Add user message
   - Add assistant message
   - Get conversation by ID
   - Get all conversations
   - Get messages for conversation (verify order)
   - Update conversation timestamp
   - Delete conversation
   - Verify 404 after deletion

2. `ConversationWorkflow_MultipleMessages_ShouldMaintainOrder` - Message ordering
3. `ConversationWorkflow_MultipleConversations_ShouldTrackSeparately` - Multiple conversations
4. `CreateConversation_WithEmptyTitle_ShouldSucceed` - Empty title allowed
5. `GetConversation_WithInvalidId_ShouldReturn404` - Error handling
6. `DeleteConversation_WithInvalidId_ShouldReturn404` - Error handling
7. `AddMessage_WithInvalidConversationId_ShouldReturn404` - Error handling
8. `AddMessage_WithMismatchedConversationId_ShouldReturnBadRequest` - Validation
9. `GetMessages_ForEmptyConversation_ShouldReturnEmptyList` - Empty messages list
10. `UpdateConversationTimestamp_WithInvalidId_ShouldReturn404` - Error handling

## Statistics

- **Total E2E Tests:** 24
- **Test Files:** 3 workflow test files
- **Infrastructure Files:** 4 support files
- **Coverage:** Complete user workflows for Questions, Randomizations, and Conversations

## Running the Tests

### Prerequisites

⚠️ **IMPORTANT:** These E2E tests require either:
1. **Firebase Emulator** running locally (recommended for true E2E testing)
2. **Real Firebase Firestore** connection configured
3. **Mock repositories** (current setup - limited functionality)

The current setup uses `Testing:SkipFirebase = true` which skips Firebase initialization. This means:
- Repositories will be registered but may not function without real Firestore
- Tests will fail when trying to interact with the database
- Tests are currently **designed but not fully operational**

### To run with Firebase Emulator:

1. Install Firebase Tools:
   ```bash
   npm install -g firebase-tools
   ```

2. Initialize Firebase in your project (if not done):
   ```bash
   firebase init emulators
   ```

3. Start the Firestore emulator:
   ```bash
   firebase emulators:start --only firestore
   ```

4. Update `E2ETestWebApplicationFactory.cs` to connect to emulator:
   - Remove `Testing:SkipFirebase = true`
   - Set `FIRESTORE_EMULATOR_HOST` environment variable to `localhost:8080`

5. Run tests:
   ```bash
   dotnet test
   ```

### Current Limitations

- Tests are **not currently functional** without a real Firestore connection
- Repositories require FirestoreDb instance to operate
- Tests will throw NullReferenceException when accessing database

### Future Enhancements

To make these tests fully operational:

1. **Option A: Use Firebase Emulator**
   - Update E2ETestWebApplicationFactory to configure Firestore emulator
   - Use Testcontainers to start/stop emulator in tests
   - Requires Docker to be running

2. **Option B: Use In-Memory Repository Implementations**
   - Create in-memory implementations of all repositories
   - Swap real repositories with in-memory versions in test configuration
   - No external dependencies required

3. **Option C: Use Real Firebase Test Project**
   - Create a dedicated Firebase project for testing
   - Configure credentials in test environment
   - Clean up data before/after test runs

## Test Patterns

### HTTP Helper Usage

```csharp
// GET request
var question = await GetAsync<QuestionDto>("/api/questions/123");

// POST request
var created = await PostAsync<CreateRequest, QuestionDto>("/api/questions", request);

// PUT request
var updated = await PutAsync<UpdateRequest, QuestionDto>("/api/questions/123", request);

// DELETE request
var response = await DeleteAsync("/api/questions/123");
```

### Assertions

```csharp
// Success assertions
response.IsSuccessStatusCode.Should().BeTrue();
AssertSuccess(response);

// Status code assertions
AssertNotFound(response);
AssertBadRequest(response);
response.StatusCode.Should().Be(HttpStatusCode.OK);
```

### Test Data Generation

```csharp
var testString = CreateTestString("Prefix"); // "Prefix_20250130123456789"
await WaitAsync(100); // Wait 100ms for timestamp differentiation
```

## Best Practices

1. **Test Isolation** - Each test should be independent
2. **Complete Workflows** - Tests should cover entire user journeys, not just single operations
3. **Error Cases** - Include tests for error scenarios (404, 400, validation failures)
4. **Assertions** - Use comprehensive assertions to verify all aspects of responses
5. **Cleanup** - Clean up test data (when using real database)

## Contributing

When adding new E2E tests:

1. Create tests in appropriate workflow file (or create new workflow file)
2. Follow existing patterns for HTTP requests and assertions
3. Test both happy path and error scenarios
4. Document complex test scenarios in comments
5. Ensure tests are independent and can run in any order

## Related Documentation

- [TESTING.md](../../docs/TESTING.md) - Overall testing strategy
- [Integration Tests](../QuestionRandomizer.IntegrationTests.Controllers/) - Integration test documentation
- [CLAUDE.md](../../CLAUDE.md) - Developer guide with architecture overview
