# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["QuestionRandomizer.slnx", "./"]
COPY ["src/QuestionRandomizer.SharedKernel/QuestionRandomizer.SharedKernel.csproj", "src/QuestionRandomizer.SharedKernel/"]
COPY ["src/Modules/QuestionRandomizer.Modules.Questions/QuestionRandomizer.Modules.Questions.csproj", "src/Modules/QuestionRandomizer.Modules.Questions/"]
COPY ["src/Modules/QuestionRandomizer.Modules.Conversations/QuestionRandomizer.Modules.Conversations.csproj", "src/Modules/QuestionRandomizer.Modules.Conversations/"]
COPY ["src/Modules/QuestionRandomizer.Modules.Randomization/QuestionRandomizer.Modules.Randomization.csproj", "src/Modules/QuestionRandomizer.Modules.Randomization/"]
COPY ["src/Modules/QuestionRandomizer.Modules.Agent/QuestionRandomizer.Modules.Agent.csproj", "src/Modules/QuestionRandomizer.Modules.Agent/"]
COPY ["src/QuestionRandomizer.Api.Controllers/QuestionRandomizer.Api.Controllers.csproj", "src/QuestionRandomizer.Api.Controllers/"]

# Restore dependencies
RUN dotnet restore "src/QuestionRandomizer.Api.Controllers/QuestionRandomizer.Api.Controllers.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/src/QuestionRandomizer.Api.Controllers"
RUN dotnet build "QuestionRandomizer.Api.Controllers.csproj" -c Release -o /app/build
RUN dotnet publish "QuestionRandomizer.Api.Controllers.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

# Expose port (5000 for Controllers API)
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "QuestionRandomizer.Api.Controllers.dll"]
