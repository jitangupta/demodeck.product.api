using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Tests.Services;

public class TenantContextServiceTests
{
    [Fact]
    public void Current_AfterSetContext_ReturnsSameContext()
    {
        var service = new TenantContextService();
        var context = new TenantContext
        {
            TenantId = "tnt_test001",
            TenantName = "test",
            UserId = "user1",
            UserRole = "Admin"
        };

        service.SetContext(context);

        service.Current.Should().BeSameAs(context);
    }

    [Fact]
    public void Current_WithoutSetContext_ThrowsInvalidOperationException()
    {
        var service = new TenantContextService();

        var act = () => service.Current;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Tenant context not set");
    }

    [Fact]
    public void SetContext_OverwritesPreviousContext()
    {
        var service = new TenantContextService();
        var first = new TenantContext { TenantId = "tnt_first" };
        var second = new TenantContext { TenantId = "tnt_second" };

        service.SetContext(first);
        service.SetContext(second);

        service.Current.Should().BeSameAs(second);
        service.Current.TenantId.Should().Be("tnt_second");
    }
}
