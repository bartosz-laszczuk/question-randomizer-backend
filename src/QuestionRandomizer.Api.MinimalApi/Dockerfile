# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["QuestionRandomizer.sln", "./"]
COPY ["src/QuestionRandomizer.Domain/QuestionRandomizer.Domain.csproj", "src/QuestionRandomizer.Domain/"]
COPY ["src/QuestionRandomizer.Application/QuestionRandomizer.Application.csproj", "src/QuestionRandomizer.Application/"]
COPY ["src/QuestionRandomizer.Infrastructure/QuestionRandomizer.Infrastructure.csproj", "src/QuestionRandomizer.Infrastructure/"]
COPY ["src/QuestionRandomizer.Api.MinimalApi/QuestionRandomizer.Api.MinimalApi.csproj", "src/QuestionRandomizer.Api.MinimalApi/"]

# Restore dependencies
RUN dotnet restore "src/QuestionRandomizer.Api.MinimalApi/QuestionRandomizer.Api.MinimalApi.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/src/QuestionRandomizer.Api.MinimalApi"
RUN dotnet build "QuestionRandomizer.Api.MinimalApi.csproj" -c Release -o /app/build
RUN dotnet publish "QuestionRandomizer.Api.MinimalApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 appuser && \
    adduser --system --uid 1001 --ingroup appuser appuser

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port (5001 for Minimal API)
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "QuestionRandomizer.Api.MinimalApi.dll"]
