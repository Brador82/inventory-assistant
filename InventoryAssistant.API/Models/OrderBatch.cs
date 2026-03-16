using System;
using System.Collections.Generic;

namespace InventoryAssistant.API.Models
{
    public class OrderBatch
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = string.Empty;

        // Draft / Submitted / Complete
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; } = string.Empty;

        // Navigation
        public ICollection<Part> Parts { get; set; } = new List<Part>();
    }
}
