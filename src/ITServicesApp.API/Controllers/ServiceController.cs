using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ServiceController : ControllerBase
    {
        private readonly IServiceCatalogService _catalog;

        public ServiceController(IServiceCatalogService catalog) => _catalog = catalog;

        // Public catalog
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ServiceCategoryDto>>> ListCategories(CancellationToken ct)
            => Ok(await _catalog.ListCategoriesAsync(ct));

        [HttpGet("categories/{categoryId:int}/issues")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ServiceIssueDto>>> ListIssues(int categoryId, CancellationToken ct)
            => Ok(await _catalog.ListIssuesByCategoryAsync(categoryId, ct));

        // Admin management
        [HttpPost("categories")]
        [Authorize] // add policy to restrict to admins
        public async Task<ActionResult<ServiceCategoryDto>> CreateCategory([FromBody] CreateServiceCategoryDto dto, CancellationToken ct)
            => Ok(await _catalog.CreateCategoryAsync(dto, ct));

        [HttpPut("categories/{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateServiceCategoryDto dto, CancellationToken ct)
        {
            await _catalog.UpdateCategoryAsync(id, dto, ct);
            return NoContent();
        }

        [HttpDelete("categories/{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct)
        {
            await _catalog.DeleteCategoryAsync(id, ct);
            return NoContent();
        }

        [HttpPost("issues")]
        [Authorize]
        public async Task<ActionResult<ServiceIssueDto>> CreateIssue([FromBody] CreateServiceIssueDto dto, CancellationToken ct)
            => Ok(await _catalog.CreateIssueAsync(dto, ct));

        [HttpDelete("issues/{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteIssue(int id, CancellationToken ct)
        {
            await _catalog.DeleteIssueAsync(id, ct);
            return NoContent();
        }
    }
}
