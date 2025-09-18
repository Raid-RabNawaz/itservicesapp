using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IBookingDraftRepository
    {
        Task<BookingDraft?> GetAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(BookingDraft draft, CancellationToken ct = default);
        Task UpdateAsync(BookingDraft draft, CancellationToken ct = default);
        Task DeleteAsync(BookingDraft draft, CancellationToken ct = default);
    }
}
