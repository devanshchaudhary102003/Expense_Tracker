using IncomeService.DTOs;
using IncomeService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IncomeService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/incomes")]
    public class IncomeController : ControllerBase
    {
        private readonly IIncomeService _svc;
        public IncomeController(IIncomeService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string Token =>
            Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        [HttpPost]
        public async Task<ActionResult<IncomeResponseDto>> Add(CreateIncomeDto dto) =>
            Ok(await _svc.AddAsync(UserId, dto));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<IncomeResponseDto>> GetById(int id) =>
            Ok(await _svc.GetByIdAsync(id, UserId));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeResponseDto>>> GetAll() =>
            Ok(await _svc.GetByUserAsync(UserId));

        [HttpGet("source/{source}")]
        public async Task<ActionResult<IEnumerable<IncomeResponseDto>>> BySource(string source) =>
            Ok(await _svc.GetBySourceAsync(UserId, source));

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<IncomeResponseDto>>> ByDateRange(
            [FromQuery] DateTime start, [FromQuery] DateTime end) =>
            Ok(await _svc.GetByDateRangeAsync(UserId, start, end));

        [HttpGet("recurring")]
        public async Task<ActionResult<IEnumerable<IncomeResponseDto>>> Recurring() =>
            Ok(await _svc.GetRecurringAsync(UserId));

        [HttpGet("total")]
        public async Task<ActionResult<decimal>> Total() =>
            Ok(await _svc.GetTotalByUserAsync(UserId));

        [HttpGet("total/source/{source}")]
        public async Task<ActionResult<decimal>> TotalBySource(string source) =>
            Ok(await _svc.GetTotalBySourceAsync(UserId, source));

        [HttpGet("net-balance")]
        public async Task<ActionResult<NetBalanceDto>> NetBalance() =>
            Ok(await _svc.GetNetBalanceAsync(UserId, Token));

        [HttpPut("{id:int}")]
        public async Task<ActionResult<IncomeResponseDto>> Update(int id, UpdateIncomeDto dto) =>
            Ok(await _svc.UpdateAsync(id, UserId, dto));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id, UserId);
            return NoContent();
        }
    }
}