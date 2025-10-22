using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demodeck.Product.Api.Models;

namespace Demodeck.Product.Api.Services
{
    public class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = new();

        public InMemoryTaskRepository()
        {
            SeedTasks();
        }

        private void SeedTasks()
        {
            int id = 1;

            // ACME (tnt_acme001)
            _tasks.AddRange(new[]
            {
                new TaskItem
                {
                    Id = id++,
                    Title = "Prepare Q3 Report",
                    Description = "Compile sales and operations figures for Q3 and produce executive summary.",
                    AssignedTo = "john.doe",
                    Status = "InProgress",
                    TenantId = "tnt_acme001",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    DueDate = DateTime.UtcNow.AddDays(5)
                },
                new TaskItem
                {
                    Id = id++,
                    Title = "Security Audit",
                    Description = "Run internal security audit for production environment.",
                    AssignedTo = "mike.wilson",
                    Status = "Open",
                    TenantId = "tnt_acme001",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    DueDate = DateTime.UtcNow.AddDays(14)
                }
            });

            // GlobalX (tnt_globalx001)
            _tasks.AddRange(new[]
            {
                new TaskItem
                {
                    Id = id++,
                    Title = "Migrate API to v2",
                    Description = "Upgrade public API to v2 and update SDKs.",
                    AssignedTo = "alex.smith",
                    Status = "InProgress",
                    TenantId = "tnt_globalx001",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    DueDate = DateTime.UtcNow.AddDays(10)
                },
                new TaskItem
                {
                    Id = id++,
                    Title = "Onboarding Session",
                    Description = "Conduct onboarding session for new hires in engineering.",
                    AssignedTo = "emma.davis",
                    Status = "Done",
                    TenantId = "tnt_globalx001",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    DueDate = DateTime.UtcNow.AddDays(-10)
                }
            });

            // Random sample task (different tenant)
            _tasks.Add(new TaskItem
            {
                Id = id++,
                Title = "Cleanup Temp Files",
                Description = "Remove stale temporary files from file store.",
                AssignedTo = "system",
                Status = "Open",
                TenantId = "tnt_sample001",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });
        }

        public Task<List<TaskItem>> GetTasksByTenantAsync(string tenantId)
        {
            var list = _tasks.Where(t => t.TenantId == tenantId).ToList();
            return Task.FromResult(list);
        }
    }
}