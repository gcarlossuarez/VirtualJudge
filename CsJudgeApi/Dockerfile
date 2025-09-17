# 🔹 What does this Dockerfile do?
#       1. Build phase (SDK):
#           Uses the official .NET SDK image.
#           Restores dependencies and compiles your project.
#           Publishes the app in Release mode.
#       2. Runtime phase (ASP.NET Core Runtime):
#           Copies the published files to /app.
#           Prepares a /app/data directory for the submissions.db file.
#           Exposes port 80.
#           Starts your API with dotnet CsJudgeApi.dll.

# ===========================================
# 1. Build stage
# ===========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY CsJudgeApi.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Build and publish the app
RUN dotnet publish -c Release -o /app/publish

# ===========================================
# 2. Runtime stage
# ===========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Create a folder for the SQLite database
RUN mkdir -p /app/data

# Environment variables (optional, useful for EF migrations or logs)
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Expose port (matches docker-compose.yml)
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "CsJudgeApi.dll"]
