using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface ITechnicianSlotService
    {
        Task<TechnicianSlotDto> CreateAsync(CreateTechnicianSlotDto dto, CancellationToken ct = default);
        Task DeleteByStartAsync(int technicianId, DateTime startUtc, CancellationToken ct = default);
        Task<IReadOnlyList<TechnicianSlotDto>> ListDayAsync(int technicianId, DateTime dayUtc, CancellationToken ct = default);
        Task<IReadOnlyList<TechnicianSlotDto>> GetAvailableAsync(int technicianId, DateTime dayUtc, CancellationToken ct = default); 

    }
}
