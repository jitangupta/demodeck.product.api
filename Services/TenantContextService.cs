using Demodeck.Product.Api.Models;

namespace Demodeck.Product.Api.Services
{
    public class TenantContextService : ITenantContextService
    {
        private static readonly AsyncLocal<TenantContext> _tenantContext = new();

        public TenantContext Current => 
            _tenantContext.Value ?? throw new InvalidOperationException("Tenant context not set");

        public void SetContext(TenantContext context)
        {
            _tenantContext.Value = context;
        }
    }
}