using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IBookingItemRepository : IRepository<BookingItem>
    {
        Task<IReadOnlyList<BookingItem>> ListByBookingAsync(int bookingId, CancellationToken ct = default);
        Task DeleteByBookingAsync(int bookingId, CancellationToken ct = default);
    }
}
