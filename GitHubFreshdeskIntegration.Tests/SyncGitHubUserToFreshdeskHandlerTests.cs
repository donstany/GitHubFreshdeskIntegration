using GitHubFreshdeskIntegration.Application.Features.Interfaces;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands;
using GitHubFreshdeskIntegration.Domain.Entities;
using Moq;


namespace GitHubFreshdeskIntegration.Tests
{
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
        public async Task Handle_GitHubUserExists_CreatesContactIfNotExists()
        {
            // Arrange
            var gitHubUser = new GitHubUser
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "123-456-7890",
                TwitterUsername = "testuser"
            };

            var freshdeskContact = new FreshdeskContact
            {
                Name = gitHubUser.Name,
                Email = gitHubUser.Email,
                Phone = gitHubUser.Phone,
                TwitterId = gitHubUser.TwitterUsername
            };

            _gitHubServiceMock.Setup(service => service.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync(gitHubUser);

            _freshdeskServiceMock.Setup(service => service.GetContactByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((FreshdeskContact)null); // Contact does not exist

            _freshdeskServiceMock.Setup(service => service.CreateContactAsync(It.IsAny<FreshdeskContact>()))
                .ReturnsAsync(freshdeskContact);

            var command = new SyncGitHubUserToFreshdeskCommand("testuser", "subdomain");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _freshdeskServiceMock.Verify(service => service.CreateContactAsync(freshdeskContact), Times.Once);
            _freshdeskServiceMock.Verify(service => service.UpdateContactAsync(It.IsAny<long>(), It.IsAny<FreshdeskContact>()), Times.Never);
        }
    }
}
