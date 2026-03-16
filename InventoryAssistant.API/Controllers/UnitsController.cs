using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAssistant.API.Data;
using InventoryAssistant.API.Models;

namespace InventoryAssistant.API.Controllers
{
    [ApiController]
    [Route("api/units")]
    public class UnitsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UnitsController(AppDbContext db) => _db = db;

        // GET /api/units/{barcode}
        [HttpGet("{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var unit = await _db.Units
                .Include(u => u.Parts)
                .Include(u => u.History.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(u => u.Barcode == barcode);

            if (unit == null) return NotFound(new { message = "Unit not found", barcode });
            return Ok(unit);
        }

        // GET /api/units?status=WaitingForParts
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = _db.Units.AsQueryable();
            if (!string.IsNullOrEmpty(status))
                query = query.Where(u => u.Status == status);

            var units = await query
                .OrderByDescending(u => u.UpdatedAt)
                .Select(u => new
                {
                    u.Id, u.Barcode, u.ApplianceType, u.Brand, u.Model,
                    u.Status, u.Location, u.HeldFor, u.UpdatedAt
                })
                .ToListAsync();

            return Ok(units);
        }

        // POST /api/units
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Unit unit)
        {
            var exists = await _db.Units.AnyAsync(u => u.Barcode == unit.Barcode);
            if (exists) return Conflict(new { message = "A unit with this barcode already exists." });

            unit.CreatedAt = DateTime.UtcNow;
            unit.UpdatedAt = DateTime.UtcNow;
            unit.Status = string.IsNullOrEmpty(unit.Status) ? "Intake" : unit.Status;

            _db.Units.Add(unit);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByBarcode), new { barcode = unit.Barcode }, unit);
        }

        // PUT /api/units/{barcode}
        [HttpPut("{barcode}")]
        public async Task<IActionResult> Update(string barcode, [FromBody] UpdateUnitRequest req)
        {
            var unit = await _db.Units.FirstOrDefaultAsync(u => u.Barcode == barcode);
            if (unit == null) return NotFound();

            var prevStatus = unit.Status;

            unit.Status = req.Status ?? unit.Status;
            unit.Notes = req.Notes ?? unit.Notes;
            unit.Location = req.Location ?? unit.Location;
            unit.HeldFor = req.HeldFor ?? unit.HeldFor;
            unit.ApplianceType = req.ApplianceType ?? unit.ApplianceType;
            unit.Brand = req.Brand ?? unit.Brand;
            unit.Model = req.Model ?? unit.Model;
            unit.SerialNumber = req.SerialNumber ?? unit.SerialNumber;
            unit.UpdatedAt = DateTime.UtcNow;

            // Write history if status changed
            if (req.Status != null && req.Status != prevStatus)
            {
                _db.UnitHistory.Add(new UnitHistory
                {
                    UnitId = unit.Id,
                    StatusFrom = prevStatus,
                    StatusTo = req.Status,
                    Notes = req.Notes ?? string.Empty,
                    ChangedBy = req.ChangedBy ?? "Unknown",
                    ChangedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return Ok(unit);
        }

        // GET /api/units/{barcode}/history
        [HttpGet("{barcode}/history")]
        public async Task<IActionResult> GetHistory(string barcode)
        {
            var unit = await _db.Units.FirstOrDefaultAsync(u => u.Barcode == barcode);
            if (unit == null) return NotFound();

            var history = await _db.UnitHistory
                .Where(h => h.UnitId == unit.Id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return Ok(history);
        }
    }

    public class UpdateUnitRequest
    {
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? Location { get; set; }
        public string? HeldFor { get; set; }
        public string? ApplianceType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? ChangedBy { get; set; }
    }
}
