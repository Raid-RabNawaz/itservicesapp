using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/admin/settings")]
    [Authorize]
    public sealed class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settings;
        public SettingsController(ISettingsService settings) => _settings = settings;

        [HttpGet]
        public async Task<ActionResult<PlatformSettingsDto>> Get(CancellationToken ct)
            => Ok(await _settings.GetAsync(ct));

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePlatformSettingsDto dto, CancellationToken ct)
        {
            await _settings.UpdateAsync(dto, ct);
            return NoContent();
        }

        [HttpGet("templates")]
        public async Task<ActionResult<IReadOnlyList<NotificationTemplateDto>>> ListTemplates(int take = 100, int skip = 0, CancellationToken ct = default)
            => Ok(await _settings.ListTemplatesAsync(take, skip, ct));

        [HttpPost("templates")]
        public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate([FromBody] NotificationTemplateDto dto, CancellationToken ct)
            => Ok(await _settings.CreateTemplateAsync(dto, ct));

        [HttpPut("templates/{id:int}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] NotificationTemplateDto dto, CancellationToken ct)
        {
            await _settings.UpdateTemplateAsync(id, dto, ct);
            return NoContent();
        }

        [HttpDelete("templates/{id:int}")]
        public async Task<IActionResult> DeleteTemplate(int id, CancellationToken ct)
        {
            await _settings.DeleteTemplateAsync(id, ct);
            return NoContent();
        }
    }
}