using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAssistant.API.Data;
using InventoryAssistant.API.Models;

namespace InventoryAssistant.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) => _db = db;

        // POST /api/orders/generate — snapshot all Needed parts into one order batch
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOrder([FromBody] GenerateOrderRequest req)
        {
            var pendingParts = await _db.Parts
                .Include(p => p.Unit)
                .Where(p => p.Status == "Needed")
                .ToListAsync();

            if (!pendingParts.Any())
                return BadRequest(new { message = "No parts are currently pending order." });

            var batch = new OrderBatch
            {
                GeneratedBy = req.GeneratedBy ?? "Unknown",
                CreatedAt = DateTime.UtcNow,
                Status = "Draft"
            };
            _db.OrderBatches.Add(batch);
            await _db.SaveChangesAsync(); // Save to get batch.Id

            foreach (var part in pendingParts)
            {
                part.Status = "Ordered";
                part.OrderedAt = DateTime.UtcNow;
                part.OrderBatchId = batch.Id;
            }

            await _db.SaveChangesAsync();

            // Return batch with parts
            var result = await _db.OrderBatches
                .Include(o => o.Parts)
                    .ThenInclude(p => p.Unit)
                .FirstAsync(o => o.Id == batch.Id);

            return Ok(result);
        }

        // GET /api/orders — list all batches
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _db.OrderBatches
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.GeneratedBy,
                    o.Status,
                    PartCount = o.Parts.Count
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _db.OrderBatches
                .Include(o => o.Parts)
                    .ThenInclude(p => p.Unit)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return Ok(order);
        }

        // GET /api/orders/{id}/export — printable HTML order form
        [HttpGet("{id}/export")]
        public async Task<IActionResult> ExportOrder(int id)
        {
            var order = await _db.OrderBatches
                .Include(o => o.Parts.OrderBy(p => p.Manufacturer).ThenBy(p => p.PartNumber))
                    .ThenInclude(p => p.Unit)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Group by manufacturer
            var byManufacturer = order.Parts
                .GroupBy(p => string.IsNullOrEmpty(p.Manufacturer) ? "Unknown Manufacturer" : p.Manufacturer)
                .OrderBy(g => g.Key)
                .ToList();

            var rows = new System.Text.StringBuilder();
            foreach (var group in byManufacturer)
            {
                rows.Append($@"
                    <tr class='mfr-row'>
                        <td colspan='5'><strong>{System.Net.WebUtility.HtmlEncode(group.Key)}</strong></td>
                    </tr>");
                foreach (var part in group)
                {
                    rows.Append($@"
                    <tr>
                        <td>{System.Net.WebUtility.HtmlEncode(part.PartNumber)}</td>
                        <td>{System.Net.WebUtility.HtmlEncode(part.PartDescription)}</td>
                        <td style='text-align:center'>{part.QuantityNeeded}</td>
                        <td>{System.Net.WebUtility.HtmlEncode(part.Unit?.Barcode ?? "")}</td>
                        <td>{System.Net.WebUtility.HtmlEncode(part.Notes)}</td>
                    </tr>");
                }
            }

            var html = $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>Parts Order #{order.Id}</title>
<style>
  body {{ font-family: Arial, sans-serif; padding: 20px; color: #111; }}
  h1 {{ font-size: 22px; margin-bottom: 4px; }}
  .meta {{ font-size: 13px; color: #555; margin-bottom: 20px; }}
  table {{ width: 100%; border-collapse: collapse; font-size: 14px; }}
  th {{ background: #1a1a2e; color: #fff; padding: 8px 10px; text-align: left; }}
  td {{ padding: 7px 10px; border-bottom: 1px solid #ddd; }}
  tr.mfr-row td {{ background: #f0f0f0; padding-top: 14px; font-size: 15px; }}
  .footer {{ margin-top: 24px; font-size: 13px; color: #555; }}
  @media print {{ button {{ display: none; }} }}
</style>
</head>
<body>
<h1>📋 Inventory Assistant — Parts Order #{order.Id}</h1>
<div class='meta'>
  Generated: {order.CreatedAt:MMM dd, yyyy h:mm tt} &nbsp;|&nbsp;
  By: {System.Net.WebUtility.HtmlEncode(order.GeneratedBy)} &nbsp;|&nbsp;
  Total Parts: {order.Parts.Count} &nbsp;|&nbsp;
  Units Affected: {order.Parts.Select(p => p.UnitId).Distinct().Count()}
</div>
<button onclick='window.print()' style='margin-bottom:16px;padding:8px 18px;background:#1a1a2e;color:#fff;border:none;border-radius:4px;font-size:14px;cursor:pointer;'>🖨️ Print</button>
<table>
  <thead>
    <tr>
      <th>Part Number</th>
      <th>Description</th>
      <th>Qty</th>
      <th>Unit Barcode</th>
      <th>Notes</th>
    </tr>
  </thead>
  <tbody>
    {rows}
  </tbody>
</table>
<div class='footer'>Inventory Assistant &mdash; Batch #{order.Id} &mdash; {order.CreatedAt:yyyy-MM-dd}</div>
</body>
</html>";

            return Content(html, "text/html");
        }

        // PUT /api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest req)
        {
            var batch = await _db.OrderBatches.FindAsync(id);
            if (batch == null) return NotFound();
            batch.Status = req.Status;
            await _db.SaveChangesAsync();
            return Ok(batch);
        }
    }

    public class GenerateOrderRequest
    {
        public string? GeneratedBy { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
