using GitHubFreshdeskIntegration.Domain.Entities;

namespace GitHubFreshdeskIntegration.Application.Extensions
{
    public static class MappingExtensions
    {
        public static FreshdeskContact ToFreshdeskContact(this GitHubUser gitHubUser)
        {
            return new FreshdeskContact
            {
                Name = gitHubUser.Name,
                Email = gitHubUser.Email,
                Phone = gitHubUser.Phone,
                TwitterId = gitHubUser.TwitterUsername
            };
        }
    }
}
