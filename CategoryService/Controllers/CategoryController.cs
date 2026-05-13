using CategoryService.DTOs;
using CategoryService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CategoryService.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _svc;
        public CategoryController(ICategoryService svc)
        {
            _svc = svc;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Defaults are public — new users can see them before they log in.
        [AllowAnonymous]
        [HttpGet("defaults")]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> Defaults() =>
            Ok(await _svc.GetDefaultsAsync());

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> Create(CreateCategoryDto dto) =>
            Ok(await _svc.CreateAsync(UserId, dto));

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryResponseDto>> GetById(int id) =>
            Ok(await _svc.GetByIdAsync(id));

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll() =>
            Ok(await _svc.GetAllForUserAsync(UserId));

        [Authorize]
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> ByType(string type) =>
            Ok(await _svc.GetByTypeAsync(UserId, type));

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryResponseDto>> Update(int id, UpdateCategoryDto dto) =>
            Ok(await _svc.UpdateAsync(id, UserId, dto));

        [Authorize]
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _svc.DeactivateAsync(id, UserId);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id, UserId);
            return NoContent();
        }
    }
}