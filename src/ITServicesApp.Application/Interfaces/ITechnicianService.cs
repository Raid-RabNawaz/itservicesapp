using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface ITechnicianService
    {
        Task<TechnicianDto?> GetAsync(int id, CancellationToken ct = default);
        Task<TechnicianProfileDto?> GetProfileByIdAsync(int id, CancellationToken ct = default);
        Task UpdateProfileAsync(int id, UpdateTechnicianProfileDto dto, CancellationToken ct = default);

        Task<List<TechnicianSlotDto>> GetCalendarAsync(int id, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
        Task<TechnicianUnavailabilityDto> MarkUnavailabilityAsync(CreateUnavailabilityDto dto, CancellationToken ct = default);
        Task DeleteUnavailabilityAsync(int unavailabilityId, CancellationToken ct = default);
        Task<TechnicianProfileDto> GetMyProfileAsync(CancellationToken ct = default);
        Task UpdateMyProfileAsync(UpdateTechnicianProfileDto dto, CancellationToken ct = default);

    }
}
