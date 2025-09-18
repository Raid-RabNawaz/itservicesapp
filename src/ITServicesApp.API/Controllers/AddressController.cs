using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class AddressController : ControllerBase
    {
        private readonly IAddressService _addresses;
        public AddressController(IAddressService addresses) => _addresses = addresses;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AddressDto>>> List([FromQuery] int take = 50, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _addresses.ListMineAsync(take, skip, ct));

        [HttpPost]
        public async Task<ActionResult<AddressDto>> Create([FromBody] CreateAddressDto dto, CancellationToken ct)
            => Ok(await _addresses.CreateAsync(dto, ct));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressDto dto, CancellationToken ct)
        { await _addresses.UpdateAsync(id, dto, ct); return NoContent(); }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        { await _addresses.DeleteAsync(id, ct); return NoContent(); }

        [HttpPost("{id:int}/default")]
        public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
        { await _addresses.SetDefaultAsync(id, ct); return NoContent(); }
    }
}