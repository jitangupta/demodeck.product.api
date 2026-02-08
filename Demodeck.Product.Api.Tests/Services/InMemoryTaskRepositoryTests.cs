using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Tests.Services;

public class InMemoryTaskRepositoryTests
{
    private readonly InMemoryTaskRepository _repository;

    public InMemoryTaskRepositoryTests()
    {
        _repository = new InMemoryTaskRepository();
    }

    [Fact]
    public async Task GetTasksByTenant_AcmeTenant_Returns2Tasks()
    {
        var tasks = await _repository.GetTasksByTenantAsync("tnt_acme001");

        tasks.Should().HaveCount(2);
        tasks.Should().OnlyContain(t => t.TenantId == "tnt_acme001");
    }

    [Fact]
    public async Task GetTasksByTenant_GlobalXTenant_Returns2Tasks()
    {
        var tasks = await _repository.GetTasksByTenantAsync("tnt_globalx001");

        tasks.Should().HaveCount(2);
        tasks.Should().OnlyContain(t => t.TenantId == "tnt_globalx001");
    }

    [Fact]
    public async Task GetTasksByTenant_SampleTenant_Returns1Task()
    {
        var tasks = await _repository.GetTasksByTenantAsync("tnt_sample001");

        tasks.Should().HaveCount(1);
        tasks.Should().OnlyContain(t => t.TenantId == "tnt_sample001");
    }

    [Fact]
    public async Task GetTasksByTenant_NonExistent_ReturnsEmpty()
    {
        var tasks = await _repository.GetTasksByTenantAsync("tnt_nonexistent");

        tasks.Should().BeEmpty();
    }
}
