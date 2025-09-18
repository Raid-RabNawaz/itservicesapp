using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IAddressService
    {
        Task<IReadOnlyList<AddressDto>> ListMineAsync(int take, int skip, CancellationToken ct);
        Task<AddressDto> CreateAsync(CreateAddressDto dto, CancellationToken ct);
        Task UpdateAsync(int id, UpdateAddressDto dto, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
        Task SetDefaultAsync(int id, CancellationToken ct);
    }
}
