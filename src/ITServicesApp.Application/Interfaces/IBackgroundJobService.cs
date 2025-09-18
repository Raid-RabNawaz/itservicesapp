using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITServicesApp.Application.Interfaces
{
    public interface IBackgroundJobService
    {
        /// <summary>Schedule a reminder and return the underlying job id.</summary>
        Task<string> ScheduleBookingReminderAsync(int bookingId, DateTime whenUtc, CancellationToken ct = default);

        /// <summary>Cancel a previously scheduled reminder using the returned job id.</summary>
        Task CancelBookingReminderAsync(string jobId, CancellationToken ct = default);
    }
}
