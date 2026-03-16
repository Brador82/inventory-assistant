using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAssistant.API.Data;
using InventoryAssistant.API.Models;

namespace InventoryAssistant.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class PartsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PartsController(AppDbContext db) => _db = db;

        // GET /api/units/{barcode}/parts
        [HttpGet("units/{barcode}/parts")]
        public async Task<IActionResult> GetPartsForUnit(string barcode)
        {
            var unit = await _db.Units.FirstOrDefaultAsync(u => u.Barcode == barcode);
            if (unit == null) return NotFound();

            var parts = await _db.Parts
                .Where(p => p.UnitId == unit.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(parts);
        }

        // POST /api/units/{barcode}/parts
        [HttpPost("units/{barcode}/parts")]
        public async Task<IActionResult> AddPart(string barcode, [FromBody] Part part)
        {
            var unit = await _db.Units.FirstOrDefaultAsync(u => u.Barcode == barcode);
            if (unit == null) return NotFound();

            part.UnitId = unit.Id;
            part.Status = "Needed";
            part.CreatedAt = DateTime.UtcNow;

            _db.Parts.Add(part);
            await _db.SaveChangesAsync();
            return Ok(part);
        }

        // PUT /api/parts/{id}
        [HttpPut("parts/{id}")]
        public async Task<IActionResult> UpdatePart(int id, [FromBody] UpdatePartRequest req)
        {
            var part = await _db.Parts.FindAsync(id);
            if (part == null) return NotFound();

            part.Status = req.Status ?? part.Status;
            part.Notes = req.Notes ?? part.Notes;
            part.Manufacturer = req.Manufacturer ?? part.Manufacturer;
            part.PartDescription = req.PartDescription ?? part.PartDescription;
            part.QuantityNeeded = req.QuantityNeeded ?? part.QuantityNeeded;
            part.ExpectedArrival = req.ExpectedArrival ?? part.ExpectedArrival;

            if (req.Status == "Received" && part.ReceivedAt == null)
                part.ReceivedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(part);
        }

        // DELETE /api/parts/{id}
        [HttpDelete("parts/{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            var part = await _db.Parts.FindAsync(id);
            if (part == null) return NotFound();
            _db.Parts.Remove(part);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET /api/parts/pending — all parts with Status = Needed
        [HttpGet("parts/pending")]
        public async Task<IActionResult> GetPendingParts()
        {
            var parts = await _db.Parts
                .Include(p => p.Unit)
                .Where(p => p.Status == "Needed")
                .OrderBy(p => p.Manufacturer)
                .ThenBy(p => p.PartNumber)
                .Select(p => new
                {
                    p.Id,
                    p.PartNumber,
                    p.PartDescription,
                    p.Manufacturer,
                    p.QuantityNeeded,
                    p.Status,
                    p.Notes,
                    p.CreatedAt,
                    Unit = new { p.Unit.Barcode, p.Unit.ApplianceType, p.Unit.Brand, p.Unit.Model }
                })
                .ToListAsync();

            return Ok(parts);
        }
    }

    public class UpdatePartRequest
    {
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? Manufacturer { get; set; }
        public string? PartDescription { get; set; }
        public int? QuantityNeeded { get; set; }
        public DateTime? ExpectedArrival { get; set; }
    }
}
