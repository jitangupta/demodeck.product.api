using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Demodeck.Product.Api.Controllers;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<ITenantContextService> _mockTenantContext;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockTenantContext = new Mock<ITenantContextService>();
        _mockLogger = new Mock<ILogger<UserController>>();

        _mockTenantContext.Setup(x => x.Current).Returns(new TenantContext
        {
            TenantId = "tnt_acme001",
            TenantName = "acme",
            UserId = "user1",
            UserRole = "Admin"
        });

        _controller = new UserController(_mockRepo.Object, _mockTenantContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUsers_Success_ReturnsOkWithUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "john", TenantId = "tnt_acme001" },
            new() { Id = 2, Username = "jane", TenantId = "tnt_acme001" }
        };
        _mockRepo.Setup(r => r.GetUsersByTenantAsync("tnt_acme001")).ReturnsAsync(users);

        var result = await _controller.GetUsers();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<User>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Message.Should().Contain("2 users");
    }

    [Fact]
    public async Task GetUsers_EmptyTenant_ReturnsOkWithEmptyList()
    {
        _mockRepo.Setup(r => r.GetUsersByTenantAsync("tnt_acme001")).ReturnsAsync(new List<User>());

        var result = await _controller.GetUsers();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<User>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsers_RepositoryThrows_Returns500()
    {
        _mockRepo.Setup(r => r.GetUsersByTenantAsync("tnt_acme001"))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _controller.GetUsers();

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        var response = statusResult.Value.Should().BeOfType<ApiResponse<List<User>>>().Subject;
        response.Success.Should().BeFalse();
        response.ErrorCode.Should().Be("USERS_RETRIEVAL_ERROR");
    }

    [Fact]
    public async Task GetUsers_Success_LogsInformation()
    {
        _mockRepo.Setup(r => r.GetUsersByTenantAsync("tnt_acme001")).ReturnsAsync(new List<User>());

        await _controller.GetUsers();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
