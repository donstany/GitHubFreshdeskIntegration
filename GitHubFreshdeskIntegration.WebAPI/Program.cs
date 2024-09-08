using FluentValidation.AspNetCore;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Validators;
using GitHubFreshdeskIntegration.Infrastructure.Services;
using GitHubFreshdeskIntegration.WebAPI.Middleware;
using System.Reflection;
using FluentValidation;
using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Application.Features.Authentication.Commands;
using GitHubFreshdeskIntegration.WebAPI.Extensions;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddRefitClients(builder.Configuration);

// Register MediatR services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SyncGitHubUserToFreshdeskHandler>());

// Register FluentValidation services
builder.Services.AddValidatorsFromAssemblyContaining<SyncGitHubUserToFreshdeskCommandValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddTransient<IGitHubService, GitHubService>();
builder.Services.AddTransient<IFreshdeskService, FreshdeskService>();
builder.Services.AddLogging();

var app = builder.Build();

// Use exception handling middleware in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<GlobalExceptionMiddleware>();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GitHubFreshdeskIntegration API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Apply CORS policy
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
