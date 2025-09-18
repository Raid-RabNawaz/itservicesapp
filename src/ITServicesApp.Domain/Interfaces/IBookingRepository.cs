using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<bool> HasOverlapAsync(int technicianId, DateTime startUtc, DateTime endUtc, CancellationToken ct);
        Task<List<Booking>> ListForUserAsync(int userId, CancellationToken ct = default);
    }
}
