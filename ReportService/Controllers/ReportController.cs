using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.DTOs;
using ReportService.Interfaces;
using System.Security.Claims;

namespace ReportService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _svc;
        public ReportController(IReportService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string Token =>
            Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        [HttpGet("monthly/{year:int}/{month:int}")]
        public async Task<ActionResult<MonthlySummaryDto>> Monthly(int year, int month) =>
            Ok(await _svc.GetMonthlySummaryAsync(UserId, year, month, Token));

        [HttpGet("category-breakdown")]
        public async Task<ActionResult<IEnumerable<CategoryBreakdownItem>>> CategoryBreakdown([FromQuery] DateTime start, [FromQuery] DateTime end) =>
            Ok(await _svc.GetCategoryBreakdownAsync(UserId, start, end, Token));

        [HttpGet("trend/{months:int}")]
        public async Task<ActionResult<IEnumerable<TrendPoint>>> Trend(int months) =>
            Ok(await _svc.GetTrendAsync(UserId, months, Token));

        [HttpGet("savings-rate/{year:int}/{month:int}")]
        public async Task<ActionResult<SavingsRateDto>> Savings(int year, int month) =>
            Ok(await _svc.GetSavingsRateAsync(UserId, year, month, Token));

        [HttpGet("yearly/{year:int}")]
        public async Task<ActionResult<YearlySummaryDto>> Yearly(int year) =>
            Ok(await _svc.GetYearlySummaryAsync(UserId, year, Token));

        [HttpGet("top-categories/{limit:int}")]
        public async Task<ActionResult<IEnumerable<CategoryBreakdownItem>>> TopCategories(int limit) =>
            Ok(await _svc.GetTopCategoriesAsync(UserId, limit, Token));

        [HttpGet("my-reports")]
        public async Task<ActionResult<IEnumerable<ReportResponseDto>>> MyReports() =>
            Ok(await _svc.GetReportsByUserAsync(UserId));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteReportAsync(id, UserId);
            return NoContent();
        }
    }
}