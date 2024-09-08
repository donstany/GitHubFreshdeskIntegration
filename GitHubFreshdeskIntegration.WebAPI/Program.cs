using GitHubFreshdeskIntegration.Application.Features.Interfaces;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands;
using GitHubFreshdeskIntegration.Infrastructure.Services;

using Refit;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);
// Register MediatR services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SyncGitHubUserToFreshdeskHandler>());

// Load tokens from environment variables
//var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
//var freshdeskToken = Environment.GetEnvironmentVariable("FRESHDESK_TOKEN");

var githubToken = "test_token";
var freshdeskToken = "test_fresh";

if (string.IsNullOrEmpty(githubToken))
{
    throw new InvalidOperationException("GitHub token is not set in the GITHUB_TOKEN environment variable.");
}

if (string.IsNullOrEmpty(freshdeskToken))
{
    throw new InvalidOperationException("Freshdesk token is not set in the FRESHDESK_TOKEN environment variable.");
}

// Add services to the container.

builder.Services.AddControllers();

// Configure GitHub Refit client with Bearer token
builder.Services.AddRefitClient<IGitHubApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://api.github.com");
        c.DefaultRequestHeaders.Add("User-Agent", "GitHubFreshdeskIntegration");
        c.DefaultRequestHeaders.Add("Authorization", $"Bearer {githubToken}");
    });

// Configure Freshdesk Refit client with Basic Auth
builder.Services.AddRefitClient<IFreshdeskApi>()
    .ConfigureHttpClient(c =>
    {
        var freshdeskSubdomain = builder.Configuration["Freshdesk:Subdomain"];
        c.BaseAddress = new Uri($"https://{freshdeskSubdomain}.freshdesk.com/api/v2");
        var encodedToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{freshdeskToken}:X"));
        c.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedToken}");
    });

builder.Services.AddTransient<IGitHubService, GitHubService>();
builder.Services.AddTransient<IFreshdeskService, FreshdeskService>();

// Add Swagger for testing (optional)
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
