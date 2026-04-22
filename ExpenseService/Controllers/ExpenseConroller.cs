using System.Security.Claims;
using ExpenseService.DTOs;
using ExpenseService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Conrollers
{
    [ApiController]
    [Authorize]
    [Route("api/expenses")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _svc;
        public ExpenseController(IExpenseService svc)
        {
            _svc = svc;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<ActionResult<ExpenseResponseDTO>> Add(CreateExpenseDTO dto) =>
            Ok(await _svc.AddExpenseAsync(UserId,dto));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExpenseResponseDTO>> GetById(int id) =>
            Ok(await _svc.GetByIdAsync(id, UserId));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDTO>>> GetAll() =>
            Ok(await _svc.GetByUserAsync(UserId));

        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDTO>>> GetByCategory(int categoryId) => 
            Ok(await _svc.GetByCategoryAsync(UserId,categoryId));

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDTO>>> ByDateRange(
            [FromQuery] DateTime start, [FromQuery] DateTime end) =>
            Ok(await _svc.GetByDateRangeAsync(UserId,start,end));

        [HttpGet("payment-mode/{mode}")]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDTO>>> ByPaymentMode(string mode) => 
            Ok(await _svc.GetByPaymentModeAsync(UserId,mode));

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDTO>>> Search([FromQuery] string q) => 
            Ok(await _svc.GetRecurringAsync(UserId));

        [HttpGet("recurring")]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDTO>>> Recurring() =>
            Ok(await _svc.GetRecurringAsync(UserId));

        [HttpGet("total")]
        public async Task<ActionResult<decimal>> Total() =>
            Ok(await _svc.GetTotalByUserAsync(UserId));

        [HttpGet("total/category/{categoryId:int}")]
        public async Task<ActionResult<decimal>> TotalByCategory(int categoryId) => 
            Ok(await _svc.GetTotalByCategoryAsync(UserId, categoryId));

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ExpenseResponseDTO>> Update(int id, UpdateExpenseDTO dto) => 
            Ok(await _svc.UpdateAsync(id,UserId,dto));

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id,UserId);
            return NoContent();
        }
    }
}