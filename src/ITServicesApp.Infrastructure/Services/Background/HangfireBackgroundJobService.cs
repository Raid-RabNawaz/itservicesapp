using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using ITServicesApp.Application.Interfaces;

namespace ITServicesApp.Infrastructure.Services.Background
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public Task<string> ScheduleBookingReminderAsync(int bookingId, DateTime whenUtc, CancellationToken ct = default)
        {
            var id = BackgroundJob.Schedule<BookingReminderJob>(
                job => job.RunAsync(bookingId, default), whenUtc);
            return Task.FromResult(id);
        }

        public Task CancelBookingReminderAsync(string jobId, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(jobId))
                BackgroundJob.Delete(jobId);
            return Task.CompletedTask;
        }
    }
}
