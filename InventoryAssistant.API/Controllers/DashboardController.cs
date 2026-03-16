using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAssistant.API.Data;

namespace InventoryAssistant.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        // GET /api/dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var statusCounts = await _db.Units
                .GroupBy(u => u.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var pendingParts = await _db.Parts.CountAsync(p => p.Status == "Needed");
            var orderedParts = await _db.Parts.CountAsync(p => p.Status == "Ordered");
            var totalUnits = await _db.Units.CountAsync();

            // Recent activity (last 10 history entries)
            var recentActivity = await _db.UnitHistory
                .Include(h => h.Unit)
                .OrderByDescending(h => h.ChangedAt)
                .Take(10)
                .Select(h => new
                {
                    h.ChangedAt,
                    h.StatusFrom,
                    h.StatusTo,
                    h.ChangedBy,
                    Barcode = h.Unit.Barcode,
                    Brand = h.Unit.Brand,
                    ApplianceType = h.Unit.ApplianceType
                })
                .ToListAsync();

            return Ok(new
            {
                TotalUnits = totalUnits,
                PendingParts = pendingParts,
                OrderedParts = orderedParts,
                StatusCounts = statusCounts,
                RecentActivity = recentActivity
            });
        }
    }
}
