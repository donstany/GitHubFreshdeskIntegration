# Use the official .NET 8 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set environment variables
# GITHUB_TOKEN is real generated from donstany account and expires 10.10.2024
ENV GITHUB_TOKEN=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
ENV FRESHDESK_TOKEN=your_freshdesk_api_key

# Set the working directory
WORKDIR /app

# Copy the solution file
COPY GitHubFreshdeskIntegration.sln ./

# Copy all project files
COPY GitHubFreshdeskIntegration.WebAPI/ GitHubFreshdeskIntegration.WebAPI/
COPY GitHubFreshdeskIntegration.Application/ GitHubFreshdeskIntegration.Application/
COPY GitHubFreshdeskIntegration.Domain/ GitHubFreshdeskIntegration.Domain/
COPY GitHubFreshdeskIntegration.Infrastructure/ GitHubFreshdeskIntegration.Infrastructure/
COPY GitHubFreshdeskIntegration.Tests/ GitHubFreshdeskIntegration.Tests/

# Restore the project dependencies
RUN dotnet restore

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish GitHubFreshdeskIntegration.WebAPI/ -c Release -o /app/publish

# Use the official .NET 8 ASP.NET runtime image as the base image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set the working directory
WORKDIR /app

# Copy the published application from the previous stage
COPY --from=publish /app/publish .

# Expose the port the app runs on
EXPOSE 7000

# Define the entry point for the application
ENTRYPOINT ["dotnet", "GitHubFreshdeskIntegration.WebAPI.dll"]
