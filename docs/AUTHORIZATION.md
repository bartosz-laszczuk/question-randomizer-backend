# Authorization System Documentation

**Last Updated:** 2025-12-21
**Version:** 1.0.0

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Roles and Permissions](#roles-and-permissions)
- [Implementation Details](#implementation-details)
- [Usage Guide](#usage-guide)
- [Code Examples](#code-examples)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)

---

## Overview

The Question Randomizer Backend implements a comprehensive role-based authorization system using:
- **3-tier role hierarchy** (User → PremiumUser → Admin)
- **Policy-based authorization** via ASP.NET Core
- **Firebase custom claims** for role storage
- **Multi-layer protection** (endpoint, handler, data level)

### Why This Authorization Model?

1. **Scalability** - Easy to add new roles and permissions
2. **Security** - Multiple layers of defense (defense in depth)
3. **Flexibility** - Supports both Controllers and Minimal API
4. **User Isolation** - Users can only access their own data (unless admin)
5. **Future-Ready** - Prepared for premium features and organization support

---

## Architecture

### Authorization Layers

```
┌─────────────────────────────────────────────────────────┐
│  Layer 1: Endpoint Protection                          │
│  - [Authorize(Policy = "UserPolicy")]                  │
│  - .RequireAuthorization(AuthorizationPolicies.Admin)  │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│  Layer 2: Handler Authorization                        │
│  - AuthorizedHandlerBase methods                       │
│  - EnsureOwnership(), EnsureAdmin(), EnsurePremium()   │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│  Layer 3: Data Filtering                               │
│  - Repository queries filtered by userId               │
│  - GetByUserIdAsync(userId, ...)                       │
└─────────────────────────────────────────────────────────┘
```

### Request Flow

```
1. Client → HTTP Request with JWT token in Authorization header
2. ASP.NET Core Middleware → UseAuthentication() validates JWT
3. Firebase JWT Validator → Extracts claims (userId, email, role)
4. UseAuthorization() → Checks policy requirements
5. Controller/Endpoint → Executes if authorized
6. Handler → Uses ICurrentUserService to get user context
7. Repository → Filters data by userId (unless admin)
8. Response → Returns only authorized data
```

---

## Roles and Permissions

### Role Definitions

| Role | Description | Use Case | Claim Value |
|------|-------------|----------|-------------|
| **User** | Standard authenticated user | 95% of users - manages own resources | `"User"` |
| **PremiumUser** | Enhanced tier with premium features | Paid subscribers, unlimited features | `"PremiumUser"` |
| **Admin** | Platform administrator | System management, support, analytics | `"Admin"` |

### Role Hierarchy

```
Admin (inherits all permissions)
  ├─ Can access any resource (bypass ownership checks)
  ├─ Access to admin-only endpoints
  ├─ Can manage other users' data
  └─ All PremiumUser features
     ↓
PremiumUser (inherits User permissions)
  ├─ Unlimited questions
  ├─ Advanced AI features (streaming)
  ├─ All User features
  └─ Future: team collaboration, advanced analytics
     ↓
User (base permissions)
  ├─ Manage own questions
  ├─ Manage own categories/qualifications
  ├─ Basic AI features
  └─ Limited to 100 questions (configurable)
```

### Permission Matrix

| Resource | User | PremiumUser | Admin |
|----------|------|-------------|-------|
| **Questions** | | | |
| Create own | ✅ | ✅ | ✅ |
| Read own | ✅ | ✅ | ✅ |
| Update own | ✅ | ✅ | ✅ |
| Delete own | ✅ | ✅ | ✅ |
| Read all users | ❌ | ❌ | ✅ |
| Max questions | 100 | Unlimited | Unlimited |
| **Categories & Qualifications** | | | |
| Manage own | ✅ | ✅ | ✅ |
| **AI Agent** | | | |
| Execute basic tasks | ✅ | ✅ | ✅ |
| Streaming responses | ❌ | ✅ | ✅ |
| Advanced features | ❌ | ✅ | ✅ |
| **Conversations** | | | |
| Manage own | ✅ | ✅ | ✅ |
| **Admin Operations** | | | |
| View analytics | ❌ | ❌ | ✅ |
| Manage users | ❌ | ❌ | ✅ |
| System health | ❌ | ❌ | ✅ |

---

## Implementation Details

### File Structure

```
src/
├── QuestionRandomizer.Infrastructure/
│   ├── Authorization/
│   │   └── AuthorizationPolicies.cs          # Role & policy definitions
│   └── Services/
│       ├── CurrentUserService.cs             # User context service
│       └── UserManagementService.cs          # Role management via Firebase
│
├── QuestionRandomizer.Application/
│   ├── Interfaces/
│   │   └── ICurrentUserService.cs            # User service interface
│   └── Common/
│       └── AuthorizedHandlerBase.cs          # Base class for handlers
│
├── QuestionRandomizer.Api.Controllers/
│   ├── Program.cs                            # Policy configuration
│   └── Controllers/
│       ├── QuestionsController.cs            # [Authorize(Policy = "UserPolicy")]
│       ├── AdminController.cs                # [Authorize(Policy = "AdminPolicy")]
│       └── ... (all controllers)
│
└── QuestionRandomizer.Api.MinimalApi/
    ├── Program.cs                            # Policy configuration
    └── Endpoints/
        ├── QuestionEndpoints.cs              # .RequireAuthorization(UserPolicy)
        ├── AdminEndpoints.cs                 # .RequireAuthorization(AdminPolicy)
        └── ... (all endpoints)
```

### Key Classes

#### 1. **AuthorizationPolicies.cs**
Location: `src/QuestionRandomizer.Infrastructure/Authorization/AuthorizationPolicies.cs`

```csharp
public static class AuthorizationPolicies
{
    // Policy names
    public const string UserPolicy = "UserPolicy";
    public const string PremiumUserPolicy = "PremiumUserPolicy";
    public const string AdminPolicy = "AdminPolicy";

    // Role names (match Firebase custom claims)
    public const string UserRole = "User";
    public const string PremiumUserRole = "PremiumUser";
    public const string AdminRole = "Admin";

    // Permission constants
    public static class Permissions
    {
        public const string QuestionsReadAll = "questions:read:all"; // Admin only
        public const string AgentStreaming = "agent:streaming";      // Premium+
        // ... more permissions
    }
}
```

#### 2. **ICurrentUserService**
Location: `src/QuestionRandomizer.Application/Interfaces/ICurrentUserService.cs`

```csharp
public interface ICurrentUserService
{
    string GetUserId();              // Current user ID
    string? GetUserEmail();          // Current user email
    string GetUserRole();            // Current user role
    bool IsInRole(string role);      // Check specific role
    bool IsAdmin();                  // Is admin?
    bool IsPremiumUser();            // Is premium or admin?
}
```

#### 3. **AuthorizedHandlerBase**
Location: `src/QuestionRandomizer.Application/Common/AuthorizedHandlerBase.cs`

```csharp
public abstract class AuthorizedHandlerBase
{
    protected readonly ICurrentUserService CurrentUserService;

    protected void EnsureOwnership(string resourceUserId);
    protected void EnsureAdmin();
    protected void EnsurePremium();
    protected string GetCurrentUserId();
    protected bool IsAdmin();
    protected bool IsPremium();
}
```

#### 4. **IUserManagementService**
Location: `src/QuestionRandomizer.Infrastructure/Services/UserManagementService.cs`

```csharp
public interface IUserManagementService
{
    Task SetUserRoleAsync(string userId, string role);
    Task MakeUserAdminAsync(string userId);
    Task UpgradeToPremiumAsync(string userId);
    Task DowngradeToUserAsync(string userId);
    Task<IDictionary<string, object>> GetUserClaimsAsync(string userId);
}
```

---

## Usage Guide

### For Developers

#### 1. **Protecting Controller Endpoints**

```csharp
using QuestionRandomizer.Infrastructure.Authorization;

// Entire controller requires authentication
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class QuestionsController : ControllerBase
{
    // All endpoints require UserPolicy
}

// Admin-only controller
[Authorize(Policy = AuthorizationPolicies.AdminPolicy)]
public class AdminController : ControllerBase
{
    // All endpoints require AdminPolicy
}

// Mixed authorization
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class AgentController : ControllerBase
{
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteBasic() { } // User+

    [HttpPost("execute/stream")]
    [Authorize(Policy = AuthorizationPolicies.PremiumUserPolicy)] // Override!
    public async Task<IActionResult> ExecuteStream() { } // Premium+ only
}
```

#### 2. **Protecting Minimal API Endpoints**

```csharp
using QuestionRandomizer.Infrastructure.Authorization;

public static void MapQuestionEndpoints(this IEndpointRouteBuilder app)
{
    // User-level endpoints
    var group = app.MapGroup("/api/questions")
        .RequireAuthorization(AuthorizationPolicies.UserPolicy)
        .WithTags("Questions");

    group.MapGet("", GetQuestions);
    group.MapPost("", CreateQuestion);
}

public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
{
    // Admin-only endpoints
    var group = app.MapGroup("/api/admin")
        .RequireAuthorization(AuthorizationPolicies.AdminPolicy)
        .WithTags("Admin");

    group.MapGet("/users", GetAllUsers);
}
```

#### 3. **Using AuthorizedHandlerBase in Handlers**

```csharp
using QuestionRandomizer.Application.Common;

public class GetQuestionsQueryHandler
    : AuthorizedHandlerBase, IRequestHandler<GetQuestionsQuery, List<QuestionDto>>
{
    private readonly IQuestionRepository _questionRepository;

    public GetQuestionsQueryHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _questionRepository = questionRepository;
    }

    public async Task<List<QuestionDto>> Handle(...)
    {
        // Get current user ID (convenience method)
        var userId = GetCurrentUserId();

        // Admin can see all questions
        if (IsAdmin())
        {
            return await _questionRepository.GetAllAsync(cancellationToken);
        }

        // Regular users see only their own
        return await _questionRepository.GetByUserIdAsync(userId, cancellationToken);
    }
}
```

#### 4. **Enforcing Ownership**

```csharp
public class UpdateQuestionCommandHandler : AuthorizedHandlerBase
{
    public async Task<QuestionDto> Handle(UpdateQuestionCommand request, ...)
    {
        // Get the question
        var question = await _questionRepository.GetByIdAsync(request.Id);

        // Ensure user owns this question (or is admin)
        EnsureOwnership(question.UserId);

        // Update the question
        // ...
    }
}
```

#### 5. **Requiring Premium Access**

```csharp
public class ExecuteAgentStreamCommandHandler : AuthorizedHandlerBase
{
    public async Task<Stream> Handle(ExecuteAgentStreamCommand request, ...)
    {
        // Ensure user is premium or admin
        EnsurePremium(); // Throws UnauthorizedException if not

        var userId = GetCurrentUserId();
        return await _agentService.ExecuteStreamAsync(request, userId);
    }
}
```

### For Administrators

#### 1. **Setting User Roles**

```csharp
// Inject IUserManagementService in your admin controller or service
private readonly IUserManagementService _userManagement;

// Make a user admin
await _userManagement.MakeUserAdminAsync("firebase-user-id");

// Upgrade to premium
await _userManagement.UpgradeToPremiumAsync("firebase-user-id");

// Downgrade to regular user
await _userManagement.DowngradeToUserAsync("firebase-user-id");

// Check user's current claims
var claims = await _userManagement.GetUserClaimsAsync("firebase-user-id");
var role = claims.ContainsKey("role") ? claims["role"].ToString() : "User";
```

#### 2. **Firebase Admin SDK Setup**

```bash
# Install Firebase CLI (if not already installed)
npm install -g firebase-tools

# Login to Firebase
firebase login

# Get service account key
# Firebase Console → Project Settings → Service Accounts → Generate New Private Key
```

**Important:** The service account key file should be referenced in `appsettings.Development.json`:

```json
{
  "Firebase": {
    "ProjectId": "your-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  }
}
```

---

## Code Examples

### Example 1: Admin Endpoint in Controllers API

```csharp
// File: src/QuestionRandomizer.Api.Controllers/Controllers/AdminController.cs

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.AdminPolicy)]
public class AdminController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetAdminInfo([FromServices] ICurrentUserService currentUserService)
    {
        return Ok(new
        {
            UserId = currentUserService.GetUserId(),
            Email = currentUserService.GetUserEmail(),
            Role = currentUserService.GetUserRole(),
            IsAdmin = currentUserService.IsAdmin()
        });
    }
}
```

### Example 2: Admin Endpoint in Minimal API

```csharp
// File: src/QuestionRandomizer.Api.MinimalApi/Endpoints/AdminEndpoints.cs

public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/admin")
        .RequireAuthorization(AuthorizationPolicies.AdminPolicy)
        .WithTags("Admin");

    group.MapGet("/me", (ICurrentUserService currentUserService) =>
    {
        return Results.Ok(new
        {
            UserId = currentUserService.GetUserId(),
            Role = currentUserService.GetUserRole(),
            IsAdmin = currentUserService.IsAdmin()
        });
    });
}
```

### Example 3: Handler with Ownership Check

```csharp
public class DeleteQuestionCommandHandler
    : AuthorizedHandlerBase, IRequestHandler<DeleteQuestionCommand>
{
    public async Task Handle(DeleteQuestionCommand request, CancellationToken ct)
    {
        // Get the question
        var question = await _questionRepository.GetByIdAsync(request.Id, ct);

        if (question == null)
        {
            throw new NotFoundException("Question not found");
        }

        // Ensure user owns this question (admins can delete any)
        EnsureOwnership(question.UserId);

        // Soft delete
        question.IsActive = false;
        await _questionRepository.UpdateAsync(question, ct);
    }
}
```

---

## Testing

### Testing with Different Roles

#### Unit Tests

```csharp
[Fact]
public async Task Handle_AdminUser_CanAccessAllQuestions()
{
    // Arrange
    var mockUserService = new Mock<ICurrentUserService>();
    mockUserService.Setup(x => x.GetUserId()).Returns("admin-user-id");
    mockUserService.Setup(x => x.IsAdmin()).Returns(true);

    var handler = new GetQuestionsQueryHandler(
        _mockQuestionRepository.Object,
        mockUserService.Object);

    // Act
    var result = await handler.Handle(new GetQuestionsQuery(), CancellationToken.None);

    // Assert
    _mockQuestionRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

#### Integration Tests

```csharp
[Fact]
public async Task GetAdminEndpoint_WithoutAdminRole_Returns403()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", _userJwtToken); // Not admin

    // Act
    var response = await client.GetAsync("/api/admin/me");

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

### Setting Up Test Users

```csharp
// Test helper to create JWT tokens with roles
public static string CreateTestJwtToken(string userId, string role = "User")
{
    // Implementation depends on your JWT generation logic
    // Must include "role" claim
}
```

---

## Troubleshooting

### Common Issues

#### 1. **401 Unauthorized - User is authenticated but getting 401**

**Cause:** JWT token doesn't have the required role claim.

**Solution:**
```bash
# Check JWT token claims at https://jwt.io
# Ensure "role" claim exists with correct value
```

```csharp
// Set role via UserManagementService
await _userManagement.SetUserRoleAsync(userId, "Admin");

// User must re-login to get new token with updated claims
```

#### 2. **403 Forbidden - User has wrong role**

**Cause:** User's role doesn't match the required policy.

**Solution:**
- Check endpoint policy: `[Authorize(Policy = AuthorizationPolicies.AdminPolicy)]`
- Check user's current role: `await _userManagement.GetUserClaimsAsync(userId)`
- Upgrade user if needed: `await _userManagement.MakeUserAdminAsync(userId)`

#### 3. **Role claim not updating**

**Cause:** JWT tokens are cached by Firebase. User needs to re-login.

**Solution:**
1. Set new role: `await _userManagement.SetUserRoleAsync(userId, "PremiumUser")`
2. Force user to re-authenticate (frontend must handle this)
3. New JWT token will include updated role claim

#### 4. **EnsureOwnership throws even for admin**

**Cause:** Logic error in handler - not checking IsAdmin() first.

**Solution:**
```csharp
// WRONG
EnsureOwnership(resource.UserId); // Always checks ownership

// CORRECT (AuthorizedHandlerBase already handles this)
EnsureOwnership(resource.UserId); // Skips check if IsAdmin() == true
```

---

## Best Practices

### Security

1. **Always validate at the endpoint level first** - Use `[Authorize]` or `.RequireAuthorization()`
2. **Add business logic checks in handlers** - Use `EnsureOwnership()`, `EnsureAdmin()`
3. **Filter data in repositories** - Always pass `userId` to queries
4. **Never trust client-provided userId** - Always use `ICurrentUserService.GetUserId()`
5. **Log authorization failures** - Track attempted unauthorized access

### Performance

1. **Cache role checks** - `ICurrentUserService` reads from HttpContext (fast)
2. **Avoid redundant ownership checks** - Use `AuthorizedHandlerBase` methods
3. **Filter early** - Apply userId filter at database level, not in memory

### Maintainability

1. **Use constants** - Reference `AuthorizationPolicies.AdminPolicy`, never hardcode strings
2. **Centralize authorization logic** - Use `AuthorizedHandlerBase` for common patterns
3. **Document role requirements** - Add XML comments to endpoints
4. **Test all authorization paths** - Unit tests for each role level

---

## Future Enhancements

### Planned Features

1. **Organization Support**
   - OrganizationOwner role
   - OrganizationMember role
   - Shared resources within organizations

2. **Resource-Based Authorization**
   - Custom authorization handlers
   - Dynamic permission checks based on resource state

3. **Audit Logging**
   - Track all authorization decisions
   - Security event logging

4. **Rate Limiting by Role**
   - User: 100 requests/min
   - Premium: 500 requests/min
   - Admin: Unlimited

---

## Related Documentation

- [SETUP-GUIDE.md](./SETUP-GUIDE.md) - Initial setup and configuration
- [CONFIGURATION.md](./CONFIGURATION.md) - Configuration details
- [TESTING.md](./TESTING.md) - Testing strategies
- [SECURITY-AUDIT.md](./SECURITY-AUDIT.md) - Security checklist
- [DUAL-API-GUIDE.md](./DUAL-API-GUIDE.md) - Controllers vs Minimal API comparison

---

## Change Log

### Version 1.0.0 (2025-12-21)
- ✅ Initial implementation
- ✅ 3-tier role system (User, PremiumUser, Admin)
- ✅ Policy-based authorization
- ✅ Firebase custom claims integration
- ✅ AuthorizedHandlerBase for handlers
- ✅ UserManagementService for role management
- ✅ Full Controllers API support
- ✅ Full Minimal API support
- ✅ Admin endpoints on both APIs
- ✅ Test infrastructure updates

---

## Support

For questions or issues related to authorization:
1. Check this documentation first
2. Review code examples in [CODE-TEMPLATES.md](./CODE-TEMPLATES.md)
3. Check existing tests in `tests/` directory
4. Refer to ASP.NET Core authorization docs: https://learn.microsoft.com/aspnet/core/security/authorization/
