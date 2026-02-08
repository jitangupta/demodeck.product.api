using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Demodeck.Product.Api.Controllers;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Tests.Controllers;

public class TaskControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepo;
    private readonly Mock<ITenantContextService> _mockTenantContext;
    private readonly Mock<ILogger<TaskController>> _mockLogger;
    private readonly TaskController _controller;

    public TaskControllerTests()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _mockTenantContext = new Mock<ITenantContextService>();
        _mockLogger = new Mock<ILogger<TaskController>>();

        _mockTenantContext.Setup(x => x.Current).Returns(new TenantContext
        {
            TenantId = "tnt_acme001",
            TenantName = "acme",
            UserId = "user1",
            UserRole = "Admin"
        });

        _controller = new TaskController(_mockRepo.Object, _mockTenantContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTasks_Success_ReturnsOkWithTasks()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", TenantId = "tnt_acme001" },
            new() { Id = 2, Title = "Task 2", TenantId = "tnt_acme001" }
        };
        _mockRepo.Setup(r => r.GetTasksByTenantAsync("tnt_acme001")).ReturnsAsync(tasks);

        var result = await _controller.GetTasks();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<TaskItem>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Message.Should().Contain("2 tasks");
    }

    [Fact]
    public async Task GetTasks_EmptyTenant_ReturnsOkWithEmptyList()
    {
        _mockRepo.Setup(r => r.GetTasksByTenantAsync("tnt_acme001")).ReturnsAsync(new List<TaskItem>());

        var result = await _controller.GetTasks();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<TaskItem>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTasks_RepositoryThrows_Returns500()
    {
        _mockRepo.Setup(r => r.GetTasksByTenantAsync("tnt_acme001"))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _controller.GetTasks();

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        var response = statusResult.Value.Should().BeOfType<ApiResponse<List<TaskItem>>>().Subject;
        response.Success.Should().BeFalse();
        response.ErrorCode.Should().Be("TASKS_RETRIEVAL_ERROR");
    }

    [Fact]
    public async Task GetTasks_Success_LogsInformation()
    {
        _mockRepo.Setup(r => r.GetTasksByTenantAsync("tnt_acme001")).ReturnsAsync(new List<TaskItem>());

        await _controller.GetTasks();

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
