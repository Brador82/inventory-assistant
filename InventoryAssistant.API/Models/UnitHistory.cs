using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryAssistant.API.Models
{
    public class UnitHistory
    {
        public int Id { get; set; }

        [Required]
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        public string StatusFrom { get; set; } = string.Empty;
        public string StatusTo { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
