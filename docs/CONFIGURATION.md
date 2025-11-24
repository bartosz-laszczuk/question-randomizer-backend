# Configuration Guide

Complete configuration reference for Question Randomizer Backend project.

---

## Configuration Files

### appsettings.json (Base Configuration)

**Location:** `src/QuestionRandomizer.Api.*/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Firebase": {
    "ProjectId": "",
    "CredentialsPath": ""
  },
  "AgentService": {
    "BaseUrl": "http://localhost:3002",
    "TimeoutSeconds": 60
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:4200"
  }
}
```

---

### appsettings.Development.json (Controllers API - Port 5000)

**Location:** `src/QuestionRandomizer.Api.Controllers/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Urls": "http://localhost:5000",
  "Firebase": {
    "ProjectId": "your-dev-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  },
  "AgentService": {
    "BaseUrl": "http://localhost:3002"
  }
}
```

---

### appsettings.Development.json (Minimal API - Port 5001)

**Location:** `src/QuestionRandomizer.Api.MinimalApi/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Urls": "http://localhost:5001",
  "Firebase": {
    "ProjectId": "your-dev-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  },
  "AgentService": {
    "BaseUrl": "http://localhost:3002"
  }
}
```

---

### appsettings.Production.json

**Location:** `src/QuestionRandomizer.Api.*/appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Firebase": {
    "ProjectId": "your-prod-project-id",
    "CredentialsPath": "/secrets/firebase-prod-credentials.json"
  },
  "AgentService": {
    "BaseUrl": "http://agent-service:3002"
  },
  "Cors": {
    "AllowedOrigins": "https://your-production-domain.com"
  }
}
```

---

## Environment Variables

### Development

```bash
# Not typically needed - use appsettings.Development.json
```

### Production

```bash
# Set in production environment (Azure App Service, Docker, etc.)
ASPNETCORE_ENVIRONMENT=Production
Firebase__ProjectId=your-prod-project-id
Firebase__CredentialsPath=/secrets/firebase-credentials.json
AgentService__BaseUrl=http://agent-service:3002
Cors__AllowedOrigins=https://your-production-domain.com
```

**Note:** Double underscore `__` in environment variables maps to nested JSON configuration.

---

## Firebase Configuration

### Firebase Credentials Setup

#### 1. Get Service Account Key

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Select your project
3. Go to **Project Settings** → **Service Accounts**
4. Click **"Generate New Private Key"**
5. Save as `firebase-dev-credentials.json` (for development)

#### 2. Store Credentials

**Development:**
- Store in project root: `firebase-dev-credentials.json`
- **IMPORTANT:** Add to `.gitignore` (never commit to Git!)

```gitignore
# .gitignore
firebase-dev-credentials.json
firebase-*.json
*.json.secret
```

**Production:**
- Use secret management service:
  - Azure Key Vault
  - Google Cloud Secret Manager
  - AWS Secrets Manager
  - HashiCorp Vault

#### 3. Configure in Code

**Infrastructure/Firebase/FirebaseInitializer.cs:**

```csharp
var credentialsPath = configuration["Firebase:CredentialsPath"];
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

FirebaseApp.Create(new AppOptions
{
    ProjectId = configuration["Firebase:ProjectId"]
});
```

---

## CORS Configuration

### Development (Allow Frontend)

```json
{
  "Cors": {
    "AllowedOrigins": "http://localhost:4200"
  }
}
```

### Production (Multiple Origins)

```json
{
  "Cors": {
    "AllowedOrigins": "https://app.example.com,https://www.example.com"
  }
}
```

**Code Implementation:**

```csharp
// Program.cs
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
```

---

## Agent Service Configuration

### Development

```json
{
  "AgentService": {
    "BaseUrl": "http://localhost:3002",
    "TimeoutSeconds": 60
  }
}
```

### Production (Internal Network)

```json
{
  "AgentService": {
    "BaseUrl": "http://agent-service:3002",
    "TimeoutSeconds": 120
  }
}
```

**Code Implementation:**

```csharp
// Infrastructure/DependencyInjection.cs
services.AddHttpClient<IAgentService, AgentService>(client =>
{
    client.BaseAddress = new Uri(configuration["AgentService:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(
        int.Parse(configuration["AgentService:TimeoutSeconds"] ?? "60"));
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

---

## OpenTelemetry Configuration

### Development (Console Exporter)

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("QuestionRandomizer.Api.Controllers"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());
```

### Production (Application Insights / OTLP)

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("QuestionRandomizer.Api.Controllers"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
        }));
```

---

## Health Checks Configuration

### Basic Health Check

```csharp
// Program.cs
builder.Services.AddHealthChecks();

app.MapHealthChecks("/health");
```

### Advanced Health Checks (with Dependencies)

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck("firebase", () =>
    {
        // Check Firebase connection
        return HealthCheckResult.Healthy();
    })
    .AddUrlGroup(new Uri(builder.Configuration["AgentService:BaseUrl"]! + "/health"),
        name: "agent-service",
        failureStatus: HealthStatus.Degraded);

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

## Logging Configuration

### Development (Verbose)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Production (Minimal)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## Database Connection Configuration

### Firestore Settings

```csharp
// Infrastructure/Firebase/FirebaseSettings.cs
public class FirebaseSettings
{
    public string ProjectId { get; set; } = string.Empty;
    public string CredentialsPath { get; set; } = string.Empty;
}

// Infrastructure/DependencyInjection.cs
services.Configure<FirebaseSettings>(configuration.GetSection("Firebase"));
```

---

## Security Configuration

### HTTPS Redirection

```csharp
// Development: Optional
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Production: Always enabled
app.UseHttpsRedirection();
```

### Authentication & Authorization

```csharp
// Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = $"https://securetoken.google.com/{builder.Configuration["Firebase:ProjectId"]}";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = $"https://securetoken.google.com/{builder.Configuration["Firebase:ProjectId"]}",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Firebase:ProjectId"],
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();
```

---

## Docker Configuration

### Dockerfile Environment Variables

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "QuestionRandomizer.Api.Controllers.dll"]
```

### Docker Compose

```yaml
version: '3.8'

services:
  api-controllers:
    build:
      context: .
      dockerfile: src/QuestionRandomizer.Api.Controllers/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Firebase__ProjectId=${FIREBASE_PROJECT_ID}
      - Firebase__CredentialsPath=/secrets/firebase-credentials.json
    volumes:
      - ./secrets/firebase-credentials.json:/secrets/firebase-credentials.json:ro

  api-minimal:
    build:
      context: .
      dockerfile: src/QuestionRandomizer.Api.MinimalApi/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Firebase__ProjectId=${FIREBASE_PROJECT_ID}
      - Firebase__CredentialsPath=/secrets/firebase-credentials.json
    volumes:
      - ./secrets/firebase-credentials.json:/secrets/firebase-credentials.json:ro
```

---

## Configuration Best Practices

### ✅ DO:
- Use different configurations for each environment
- Store secrets in secure vaults (Azure Key Vault, AWS Secrets Manager)
- Use environment variables for production secrets
- Keep development credentials separate from production
- Document all configuration options

### ❌ DON'T:
- Commit credentials to Git
- Use production credentials in development
- Hardcode secrets in code
- Share credentials in plain text
- Use same configuration for all environments

---

## Troubleshooting

### Issue: "Firebase credentials not found"
**Solution:**
1. Verify `firebase-dev-credentials.json` exists
2. Check path in `appsettings.Development.json`
3. Ensure file is not in `.gitignore` (should be, but copy it locally)

### Issue: "CORS error from frontend"
**Solution:**
1. Check `Cors:AllowedOrigins` includes frontend URL
2. Verify `app.UseCors()` is before `app.UseAuthorization()`
3. Ensure credentials are allowed: `.AllowCredentials()`

### Issue: "Agent service timeout"
**Solution:**
1. Increase `AgentService:TimeoutSeconds` in config
2. Check agent service is running
3. Verify network connectivity

---

## Configuration Validation

Add configuration validation on startup:

```csharp
// Program.cs
var firebaseSettings = builder.Configuration.GetSection("Firebase").Get<FirebaseSettings>();

if (string.IsNullOrEmpty(firebaseSettings?.ProjectId))
    throw new InvalidOperationException("Firebase ProjectId is required");

if (string.IsNullOrEmpty(firebaseSettings?.CredentialsPath))
    throw new InvalidOperationException("Firebase CredentialsPath is required");

if (!File.Exists(firebaseSettings.CredentialsPath))
    throw new FileNotFoundException($"Firebase credentials file not found: {firebaseSettings.CredentialsPath}");
```

---

## Quick Reference

```bash
# View current configuration
dotnet run --urls=http://localhost:5000

# Override configuration via environment variable
export Firebase__ProjectId="my-project-id"
dotnet run

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Staging dotnet run
```
