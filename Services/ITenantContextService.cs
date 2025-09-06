using Demodeck.Product.Api.Models;

namespace Demodeck.Product.Api.Services
{
    public interface ITenantContextService
    {
        TenantContext Current { get; }
        void SetContext(TenantContext context);
    }
}