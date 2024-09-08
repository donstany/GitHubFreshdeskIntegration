using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Validators;
using GitHubFreshdeskIntegration.Infrastructure.Services;
using GitHubFreshdeskIntegration.WebAPI.Middleware;
using Refit;
using System.Reflection;
using FluentValidation;
using GitHubFreshdeskIntegration.Application.Features.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Authentication
var jwtKey = "your_secret_key_here"; // Replace with your actual secret key
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Register MediatR services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Register FluentValidation services
builder.Services.AddValidatorsFromAssemblyContaining<SyncGitHubUserToFreshdeskCommandValidator>();
builder.Services.AddFluentValidationAutoValidation(); // Automatically validate model state
builder.Services.AddFluentValidationClientsideAdapters(); // Add client-side validation adapters if needed

// Load tokens from environment variables
var githubToken = "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"; // working credentials
var freshdeskToken = "xxx"; // mock credentials
//var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
//var freshdeskToken = Environment.GetEnvironmentVariable("FRESHDESK_TOKEN");

if (string.IsNullOrEmpty(githubToken))
{
    throw new InvalidOperationException("GitHub token is not set.");
}

if (string.IsNullOrEmpty(freshdeskToken))
{
    throw new InvalidOperationException("Freshdesk token is not set.");
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

builder.Services.AddLogging();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<GlobalExceptionMiddleware>();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
