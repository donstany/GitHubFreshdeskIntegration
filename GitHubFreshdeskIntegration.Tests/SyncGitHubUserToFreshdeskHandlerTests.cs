using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands;
using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

public class SyncGitHubUserToFreshdeskHandlerTests
{
    private readonly Mock<IGitHubService> _gitHubServiceMock;
    private readonly Mock<IFreshdeskService> _freshdeskServiceMock;
    private readonly Mock<ILogger<SyncGitHubUserToFreshdeskHandler>> _loggerMock;
    private readonly SyncGitHubUserToFreshdeskHandler _handler;

    public SyncGitHubUserToFreshdeskHandlerTests()
    {
        _gitHubServiceMock = new Mock<IGitHubService>();
        _freshdeskServiceMock = new Mock<IFreshdeskService>();
        _loggerMock = new Mock<ILogger<SyncGitHubUserToFreshdeskHandler>>();
        _handler = new SyncGitHubUserToFreshdeskHandler(_gitHubServiceMock.Object, _freshdeskServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateContact_WhenContactDoesNotExist()
    {
        // Arrange
        var command = new SyncGitHubUserToFreshdeskCommand("githubUsername", "freshdeskSubdomain");
        var cancellationToken = new CancellationToken();

        var gitHubUser = new GitHubUser
        {
            Name = "Testuser",
            Email = "user@example.com",
            Phone = "123-456-7890",
            TwitterUsername = "user_tweet"
        };

        var freshdeskContact = new FreshdeskContact
        {
            Name = gitHubUser.Name,
            Email = gitHubUser.Email,
            Phone = gitHubUser.Phone,
            TwitterId = gitHubUser.TwitterUsername
        };

        _gitHubServiceMock
            .Setup(service => service.GetUserAsync(command.Username, cancellationToken))
            .ReturnsAsync(gitHubUser);

        _freshdeskServiceMock
            .Setup(service => service.GetContactByEmailAsync(gitHubUser.Email, cancellationToken))
            .ReturnsAsync((FreshdeskContact?)null); // No existing contact

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        _freshdeskServiceMock.Verify(service => service.CreateContactAsync(It.Is<FreshdeskContact>(c => c.Email == gitHubUser.Email), cancellationToken), Times.Once);
        _freshdeskServiceMock.Verify(service => service.UpdateContactAsync(It.IsAny<long>(), It.IsAny<FreshdeskContact>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task Handle_ShouldUpdateContact_WhenContactExists()
    {
        // Arrange
        var command = new SyncGitHubUserToFreshdeskCommand("githubUsername", "freshdeskSubdomain");
        var cancellationToken = new CancellationToken();

        var gitHubUser = new GitHubUser
        {
            Name = "Testuser",
            Email = "user@example.com",
            Phone = "123-456-7890",
            TwitterUsername = "user_tweet"
        };

        var existingContact = new FreshdeskContact
        {
            Id = 1234,
            Email = gitHubUser.Email
        };

        _gitHubServiceMock
            .Setup(service => service.GetUserAsync(command.Username, cancellationToken))
            .ReturnsAsync(gitHubUser);

        _freshdeskServiceMock
            .Setup(service => service.GetContactByEmailAsync(gitHubUser.Email, cancellationToken))
            .ReturnsAsync(existingContact); // Contact already exists

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        _freshdeskServiceMock.Verify(service => service.UpdateContactAsync(existingContact.Id, It.Is<FreshdeskContact>(c => c.Email == gitHubUser.Email), cancellationToken), Times.Once);
        _freshdeskServiceMock.Verify(service => service.CreateContactAsync(It.IsAny<FreshdeskContact>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenGitHubUserNotFound()
    {
        // Arrange
        var command = new SyncGitHubUserToFreshdeskCommand("nonexistentUser", "freshdeskSubdomain");
        var cancellationToken = new CancellationToken();

        _gitHubServiceMock
            .Setup(service => service.GetUserAsync(command.Username, cancellationToken))
            .ReturnsAsync((GitHubUser?)null); // GitHub user not found

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        _freshdeskServiceMock.Verify(service => service.CreateContactAsync(It.IsAny<FreshdeskContact>(), It.IsAny<CancellationToken>()), Times.Never);
        _freshdeskServiceMock.Verify(service => service.UpdateContactAsync(It.IsAny<long>(), It.IsAny<FreshdeskContact>(), It.IsAny<CancellationToken>()), Times.Never);

        _loggerMock.Verify(
                   logger => logger.Log(
                       LogLevel.Warning,
                       It.IsAny<EventId>(), 
                       It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GitHub user 'nonexistentUser' not found")), 
                       It.IsAny<Exception>(), // Exception
                       It.IsAny<Func<It.IsAnyType, Exception, string>>()
                   ), Times.Once);

        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task Handle_ShouldCallGitHubServiceWithCorrectUsername()
    {
        // Arrange
        var command = new SyncGitHubUserToFreshdeskCommand("correctUsername", "freshdeskSubdomain");
        var cancellationToken = new CancellationToken();

        _gitHubServiceMock
            .Setup(service => service.GetUserAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(new GitHubUser());

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _gitHubServiceMock.Verify(service => service.GetUserAsync("correctUsername", cancellationToken), Times.Once);
    }
}
