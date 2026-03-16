using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryAssistant.API.Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required]
        public string Barcode { get; set; } = string.Empty;

        public string ApplianceType { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Intake";

        public string Notes { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        // For HeldForCustomer status
        public string HeldFor { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Part> Parts { get; set; } = new List<Part>();
        public ICollection<UnitHistory> History { get; set; } = new List<UnitHistory>();
    }
}
