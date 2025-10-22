using System;

namespace Demodeck.Product.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string AssignedTo { get; set; } = "";
        public string Status { get; set; } = ""; // e.g. Open, InProgress, Done
        public string TenantId { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }
}