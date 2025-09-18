using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Services
{
    public class TechnicianDashboardService : ITechnicianDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEarningsService _earnings;
        private readonly ICurrentUserService _current;

        private static readonly TimeSpan EarningsWindow = TimeSpan.FromDays(30);
        private const int MaxItems = 5;

        public TechnicianDashboardService(ApplicationDbContext db, IEarningsService earnings, ICurrentUserService current)
        {
            _db = db;
            _earnings = earnings;
            _current = current;
        }

        public async Task<TechnicianDashboardDto> GetAsync(int technicianId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default)
        {
            await EnsureTechnicianAccessAsync(technicianId, ct);

            var windowTo = toUtc ?? DateTime.UtcNow;
            var windowFrom = fromUtc ?? windowTo.Subtract(EarningsWindow);

            var earnings = await _earnings.GetSummaryAsync(technicianId, windowFrom, windowTo, ct);

            var now = DateTime.UtcNow;
            var upcomingQuery = _db.Bookings.AsNoTracking()
                .Where(b => b.TechnicianId == technicianId && b.Status != BookingStatus.Cancelled && b.ScheduledStartUtc >= now);

            var recentQuery = _db.Bookings.AsNoTracking()
                .Where(b => b.TechnicianId == technicianId && b.Status == BookingStatus.Completed && b.ScheduledEndUtc < now);

            var pendingConfirmationCount = await _db.Bookings.AsNoTracking()
                .CountAsync(b => b.TechnicianId == technicianId && b.Status == BookingStatus.PendingTechnicianConfirmation, ct);

            var completedLast30Days = await _db.Bookings.AsNoTracking()
                .CountAsync(b => b.TechnicianId == technicianId && b.Status == BookingStatus.Completed && b.ScheduledEndUtc >= now.AddDays(-30), ct);

            var upcoming = await MaterializeBookingsAsync(upcomingQuery.OrderBy(b => b.ScheduledStartUtc).Take(MaxItems), ct);
            var recent = await MaterializeBookingsAsync(recentQuery.OrderByDescending(b => b.ScheduledEndUtc).Take(MaxItems), ct);

            return new TechnicianDashboardDto
            {
                TechnicianId = technicianId,
                Earnings = earnings,
                PendingConfirmationCount = pendingConfirmationCount,
                CompletedLast30Days = completedLast30Days,
                UpcomingBookings = upcoming,
                RecentBookings = recent
            };
        }

        private async Task<IReadOnlyList<BookingSnapshotDto>> MaterializeBookingsAsync(IQueryable<Domain.Entities.Booking> query, CancellationToken ct)
        {
            return await query
                .Select(b => new BookingSnapshotDto
                {
                    BookingId = b.Id,
                    StartUtc = b.ScheduledStartUtc,
                    EndUtc = b.ScheduledEndUtc,
                    Status = b.Status,
                    ServiceName = b.ServiceIssue != null ? b.ServiceIssue.Name : null,
                    CustomerName = b.User != null ? b.User.FullName : null,
                    TechnicianName = b.Technician != null && b.Technician.User != null ? b.Technician.User.FullName : null,
                    TotalAmount = b.FinalTotal ?? b.EstimatedTotal,
                    Currency = "USD"
                })
                .ToListAsync(ct);
        }

        private async Task EnsureTechnicianAccessAsync(int technicianId, CancellationToken ct)
        {
            var role = GetRole();
            if (role == UserRole.Admin)
            {
                return;
            }

            var userId = _current.UserIdInt;
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException("Authentication required.");
            }

            var technician = await _db.Technicians.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == technicianId, ct)
                ?? throw new InvalidOperationException("Technician not found.");

            if (role == UserRole.Technician && technician.UserId == userId)
            {
                return;
            }

            throw new UnauthorizedAccessException("You cannot view this technician dashboard.");
        }

        private UserRole GetRole()
        {
            var raw = _current.Role;
            return Enum.TryParse<UserRole>(raw, true, out var parsed) ? parsed : UserRole.Customer;
        }
    }
}
