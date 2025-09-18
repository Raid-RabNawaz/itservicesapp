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
    public class CustomerDashboardService : ICustomerDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        private const int MaxItems = 5;
        private static readonly TimeSpan SpendWindow = TimeSpan.FromDays(90);

        public CustomerDashboardService(ApplicationDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        public async Task<CustomerDashboardDto> GetAsync(int customerId, CancellationToken ct = default)
        {
            EnsureCustomerAccess(customerId);

            var now = DateTime.UtcNow;

            var upcomingQuery = _db.Bookings.AsNoTracking()
                .Where(b => b.UserId == customerId && b.Status != BookingStatus.Cancelled && b.ScheduledStartUtc >= now);

            var recentQuery = _db.Bookings.AsNoTracking()
                .Where(b => b.UserId == customerId && b.ScheduledStartUtc < now)
                .OrderByDescending(b => b.ScheduledStartUtc);

            var activeRequests = await _db.Bookings.AsNoTracking()
                .CountAsync(b => b.UserId == customerId && (b.Status == BookingStatus.PendingCustomerConfirmation || b.Status == BookingStatus.PendingTechnicianConfirmation), ct);

            var spendFrom = now.Subtract(SpendWindow);
            var spent = await _db.Invoices.AsNoTracking()
                .Join(_db.Bookings.AsNoTracking(), i => i.BookingId, b => b.Id, (i, b) => new { invoice = i, booking = b })
                .Where(x => x.booking.UserId == customerId && x.invoice.IssuedAtUtc >= spendFrom && x.invoice.IssuedAtUtc <= now)
                .SumAsync(x => (decimal?)x.invoice.Total, ct) ?? 0m;

            var upcoming = await MaterializeBookingsAsync(upcomingQuery.OrderBy(b => b.ScheduledStartUtc).Take(MaxItems), ct);
            var recent = await MaterializeBookingsAsync(recentQuery.Take(MaxItems), ct);

            var currency = upcoming.FirstOrDefault()?.Currency ?? recent.FirstOrDefault()?.Currency ?? "USD";

            return new CustomerDashboardDto
            {
                CustomerId = customerId,
                ActiveRequests = activeRequests,
                TotalSpentLast90Days = Math.Round(spent, 2, MidpointRounding.AwayFromZero),
                Currency = currency,
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
                    TechnicianName = b.Technician != null && b.Technician.User != null ? b.Technician.User.FullName : null,
                    CustomerName = b.User != null ? b.User.FullName : null,
                    TotalAmount = b.FinalTotal ?? b.EstimatedTotal,
                    Currency = "USD"
                })
                .ToListAsync(ct);
        }

        private void EnsureCustomerAccess(int customerId)
        {
            var role = GetRole();
            if (role == UserRole.Admin)
            {
                return;
            }

            var currentUserId = _current.UserIdInt;
            if (currentUserId <= 0)
            {
                throw new UnauthorizedAccessException("Authentication required.");
            }

            if (currentUserId != customerId)
            {
                throw new UnauthorizedAccessException("You cannot view this customer dashboard.");
            }
        }

        private UserRole GetRole()
        {
            var raw = _current.Role;
            return Enum.TryParse<UserRole>(raw, true, out var parsed) ? parsed : UserRole.Customer;
        }
    }
}
