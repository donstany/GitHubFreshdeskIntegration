using GitHubFreshdeskIntegration.Domain.Entities;

namespace GitHubFreshdeskIntegration.Application.Features.Interfaces
{
    public interface IFreshdeskService
    {
        Task<FreshdeskContact> GetContactByEmailAsync(string email, CancellationToken cancellationToken);
        Task<FreshdeskContact> CreateContactAsync(FreshdeskContact contact, CancellationToken cancellationToken);
        Task UpdateContactAsync(long id, FreshdeskContact contact, CancellationToken cancellationToken);
    }
}
