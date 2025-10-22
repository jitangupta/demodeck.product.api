using Demodeck.Product.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demodeck.Product.Api.Services
{
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetTasksByTenantAsync(string tenantId);
    }
}