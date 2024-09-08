using Moq;
using GitHubFreshdeskIntegration.Application.Features.Interfaces;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands;
using GitHubFreshdeskIntegration.Domain.Entities;
using MediatR;

public class SyncGitHubUserToFreshdeskHandlerTests
{
    private readonly Mock<IGitHubService> _gitHubServiceMock;
    private readonly Mock<IFreshdeskService> _freshdeskServiceMock;
    private readonly SyncGitHubUserToFreshdeskHandler _handler;

    public SyncGitHubUserToFreshdeskHandlerTests()
    {
        _gitHubServiceMock = new Mock<IGitHubService>();
        _freshdeskServiceMock = new Mock<IFreshdeskService>();
        _handler = new SyncGitHubUserToFreshdeskHandler(_gitHubServiceMock.Object, _freshdeskServiceMock.Object);
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
            .ReturnsAsync((FreshdeskContact)null); // No existing contact

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
}
