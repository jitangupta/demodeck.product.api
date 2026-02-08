using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Tests.Services;

public class InMemoryUserRepositoryTests
{
    private readonly InMemoryUserRepository _repository;

    public InMemoryUserRepositoryTests()
    {
        _repository = new InMemoryUserRepository();
    }

    [Fact]
    public async Task GetUsersByTenant_AcmeTenant_Returns4Users()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_acme001");

        users.Should().HaveCount(4);
        users.Should().OnlyContain(u => u.TenantId == "tnt_acme001");
    }

    [Fact]
    public async Task GetUsersByTenant_GlobalXTenant_Returns5Users()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_globalx001");

        users.Should().HaveCount(5);
        users.Should().OnlyContain(u => u.TenantId == "tnt_globalx001");
    }

    [Fact]
    public async Task GetUsersByTenant_InitechTenant_Returns4Users()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_initech001");

        users.Should().HaveCount(4);
        users.Should().OnlyContain(u => u.TenantId == "tnt_initech001");
    }

    [Fact]
    public async Task GetUsersByTenant_UmbrellaTenant_Returns5Users()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_umbrella001");

        users.Should().HaveCount(5);
        users.Should().OnlyContain(u => u.TenantId == "tnt_umbrella001");
    }

    [Fact]
    public async Task GetUsersByTenant_NonExistent_ReturnsEmpty()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_nonexistent");

        users.Should().BeEmpty();
    }
}
