using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Controllers
{
    [ApiController]
    [Route("{tenantName}/api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ITaskRepository taskRepository, ITenantContextService tenantContext, ILogger<TaskController> logger)
        {
            _taskRepository = taskRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TaskItem>>), 200)]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var tasks = await _taskRepository.GetTasksByTenantAsync(_tenantContext.Current.TenantId);

                _logger.LogInformation("Retrieved {Count} tasks for tenant {TenantName}", tasks.Count, _tenantContext.Current.TenantName);

                return Ok(new ApiResponse<List<TaskItem>>
                {
                    Success = true,
                    Data = tasks,
                    Message = $"Retrieved {tasks.Count} tasks for tenant {_tenantContext.Current.TenantName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for tenant {TenantId}", _tenantContext.Current.TenantId);
                return StatusCode(500, new ApiResponse<List<TaskItem>>
                {
                    Success = false,
                    Message = "Error retrieving tasks",
                    ErrorCode = "TASKS_RETRIEVAL_ERROR"
                });
            }
        }
    }
}