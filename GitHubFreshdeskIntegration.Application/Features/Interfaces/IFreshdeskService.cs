using GitHubFreshdeskIntegration.Domain.Entities;

namespace GitHubFreshdeskIntegration.Application.Features.Interfaces
{
    public interface IFreshdeskService
    {
        Task<FreshdeskContact> GetContactByEmailAsync(string email);
        Task<FreshdeskContact> CreateContactAsync(FreshdeskContact contact);
        Task UpdateContactAsync(long id, FreshdeskContact contact);
    }
}
