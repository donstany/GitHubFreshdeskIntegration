# GitHubFreshdeskIntegration

## Overview

GitHubFreshdeskIntegration is a .NET 8 WebAPI application designed to integrate GitHub and Freshdesk APIs. This application retrieves GitHub user information and synchronizes it with Freshdesk by creating or updating contacts.

## Features

- **GitHub User Information Retrieval**: Uses the GitHub REST API v3 to fetch user details. [GitHub REST API documentation](https://docs.github.com/en/rest)
- **Freshdesk Contact Management**: Uses the Freshdesk API v2 to create or update contacts. [Freshdesk API documentation](https://developer.freshdesk.com/api/)
- **Clean Architecture**: Implements a well-structured solution using Clean Architecture principles.
- **CQRS and Mediator Patterns**: Utilizes MediatR and CQRS patterns for handling commands and queries.
- **Optional Authentication**: API can be secured with JWT/Bearer token if needed.
- **Configurable Endpoints**: Exposes an endpoint that accepts GitHub username and Freshdesk subdomain as parameters.

## Environment Variables

- `GITHUB_TOKEN`: Your GitHub personal access token.
- `FRESHDESK_TOKEN`: Your Freshdesk API key.

## Implementation Details

- **Field Mapping**: Transfers relevant fields from the GitHub user to the Freshdesk contact based on compatibility and relevance.
- **Unit Testing**: Includes unit tests for core functionalities to ensure reliability.
- **Optional Docker Support**: Dockerfile provided for building a Docker image of the application.

## Setup

1. **Clone the Repository**

   ```bash
   git clone https://github.com/your-username/GitHubFreshdeskIntegration.git
   cd GitHubFreshdeskIntegration
