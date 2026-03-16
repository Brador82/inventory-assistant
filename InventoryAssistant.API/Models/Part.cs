using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryAssistant.API.Models
{
    public class Part
    {
        public int Id { get; set; }

        [Required]
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        [Required]
        public string PartNumber { get; set; } = string.Empty;

        public string PartDescription { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int QuantityNeeded { get; set; } = 1;

        // Needed / Ordered / Received / Cancelled
        public string Status { get; set; } = "Needed";

        public int? OrderBatchId { get; set; }
        public OrderBatch? OrderBatch { get; set; }

        public DateTime? OrderedAt { get; set; }
        public DateTime? ExpectedArrival { get; set; }
        public DateTime? ReceivedAt { get; set; }

        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
