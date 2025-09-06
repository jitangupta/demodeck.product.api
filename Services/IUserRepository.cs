using Demodeck.Product.Api.Models;

namespace Demodeck.Product.Api.Services
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersByTenantAsync(string tenantId);
    }
}