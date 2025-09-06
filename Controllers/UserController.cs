using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Controllers
{
    [ApiController]
    [Route("{tenantName}/api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ITenantContextService tenantContext, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<User>>), 200)]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userRepository.GetUsersByTenantAsync(_tenantContext.Current.TenantId);
                
                _logger.LogInformation("Retrieved {Count} users for tenant {TenantName}", 
                    users.Count, _tenantContext.Current.TenantName);

                return Ok(new ApiResponse<List<User>>
                {
                    Success = true,
                    Data = users,
                    Message = $"Retrieved {users.Count} users for tenant {_tenantContext.Current.TenantName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for tenant {TenantId}", _tenantContext.Current.TenantId);
                return StatusCode(500, new ApiResponse<List<User>>
                {
                    Success = false,
                    Message = "Error retrieving users",
                    ErrorCode = "USERS_RETRIEVAL_ERROR"
                });
            }
        }
    }
}