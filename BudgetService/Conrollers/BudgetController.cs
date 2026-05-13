using BudgetService.DTOs;
using BudgetService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/budgets")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _svc;
        public BudgetController(IBudgetService svc)
        {
            _svc = svc;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<ActionResult<BudgetResponseDto>> Create(CreateBudgetDto dto) =>
            Ok(await _svc.CreateAsync(UserId, dto));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BudgetResponseDto>> GetById(int id) =>
            Ok(await _svc.GetByIdAsync(id, UserId));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetAll() =>
            Ok(await _svc.GetByUserAsync(UserId));

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> Active() =>
            Ok(await _svc.GetActiveAsync(UserId));

        [HttpGet("alerts")]
        public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> OverBudget() =>
            Ok(await _svc.GetOverBudgetAsync(UserId));

        [HttpGet("utilization")]
        public async Task<ActionResult<decimal>> Utilization() =>
            Ok(await _svc.GetUtilizationAsync(UserId));

        [HttpPut("{id:int}")]
        public async Task<ActionResult<BudgetResponseDto>> Update(int id, UpdateBudgetDto dto) =>
            Ok(await _svc.UpdateAsync(id, UserId, dto));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id, UserId);
            return NoContent();
        }
    }
}