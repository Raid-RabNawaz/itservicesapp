using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _db;

        public AdminDashboardService(ApplicationDbContext db) => _db = db;

        public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var grossRevenue = await _db.Payments
                .Where(p => p.Status == "Succeeded")
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
            var technicianNet = Math.Round(grossRevenue * 0.90m, 2, MidpointRounding.AwayFromZero);

            var totalBookings = await _db.Bookings.CountAsync(ct);
            var upcoming = await _db.Bookings.CountAsync(b => b.ScheduledStartUtc > now && b.Status != BookingStatus.Cancelled, ct);
            var pending = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.PendingTechnicianConfirmation || b.Status == BookingStatus.PendingCustomerConfirmation, ct);
            var completed = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Completed, ct);
            var cancelled = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled, ct);
            var activeTechnicians = await _db.Technicians.CountAsync(t => t.IsActive, ct);
            var activeCustomers = await _db.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .Select(b => b.UserId)
                .Distinct()
                .CountAsync(ct);

            return new DashboardStatsDto
            {
                TotalBookings = totalBookings,
                UpcomingBookings = upcoming,
                PendingBookings = pending,
                CompletedBookings = completed,
                CancelledBookings = cancelled,
                ActiveTechnicians = activeTechnicians,
                ActiveCustomers = activeCustomers,
                TotalRevenue = Math.Round(grossRevenue, 2, MidpointRounding.AwayFromZero),
                TechnicianNetRevenue = technicianNet
            };
        }

        public async Task<RevenueReportDto> GetRevenueAsync(DateTime fromUtc, DateTime toUtc, string interval, CancellationToken ct = default)
        {
            interval = interval?.Trim().ToLowerInvariant() ?? "daily";

            var succeeded = await _db.Payments
                .Where(p => p.Status == "Succeeded" &&
                            p.CreatedAtUtc >= fromUtc && p.CreatedAtUtc < toUtc)
                .Select(p => new { p.CreatedAtUtc, p.Amount, p.Currency })
                .ToListAsync(ct);

            string currency = succeeded.FirstOrDefault()?.Currency ?? "USD";
            var buckets = new Dictionary<DateTime, decimal>();

            DateTime keyOf(DateTime d) =>
                interval switch
                {
                    "weekly" => d.Date.AddDays(-(int)d.Date.DayOfWeek),                       // week start (Sunday)
                    "monthly" => new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc), // month start
                    _ => d.Date                                                        // daily
                };

            foreach (var p in succeeded)
            {
                var k = keyOf(p.CreatedAtUtc);
                if (!buckets.ContainsKey(k)) buckets[k] = 0m;
                buckets[k] += p.Amount;
            }

            var report = new RevenueReportDto
            {
                Currency = currency,
                Total = succeeded.Sum(x => x.Amount),
                Buckets = buckets.OrderBy(kv => kv.Key)
                                 .Select(kv => new RevenueBucketDto
                                 {
                                     PeriodStartUtc = kv.Key,
                                     Amount = kv.Value,
                                     Currency = currency
                                 }).ToList()
            };

            return report;
        }

        public async Task<IReadOnlyList<TechnicianUtilizationDto>> GetTechnicianUtilizationAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        {
            // Pull all relevant data then compute overlap durations in memory (simple & accurate).
            var techs = await _db.Technicians
                                 .Include(t => t.User)
                                 .Where(t => t.IsActive)
                                 .ToListAsync(ct);

            var slots = await _db.TechnicianSlots
                                 .Where(s => s.StartUtc < toUtc && fromUtc < s.EndUtc)
                                 .ToListAsync(ct);

            var bookings = await _db.Bookings
                                    .Where(b => b.Status != BookingStatus.Cancelled &&
                                                b.ScheduledStartUtc < toUtc && fromUtc < b.ScheduledEndUtc)
                                    .ToListAsync(ct);

            static double OverlapHours(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
            {
                var start = aStart > bStart ? aStart : bStart;
                var end = aEnd < bEnd ? aEnd : bEnd;
                var span = end - start;
                return span > TimeSpan.Zero ? span.TotalHours : 0d;
            }

            var result = new List<TechnicianUtilizationDto>();

            foreach (var t in techs)
            {
                // Available hours = sum of slot overlap within window
                var mySlots = slots.Where(s => s.TechnicianId == t.Id);
                double available = mySlots.Sum(s => OverlapHours(s.StartUtc, s.EndUtc, fromUtc, toUtc));

                // Booked hours = sum of booking overlap within available slots (conservative approach)
                var myBookings = bookings.Where(b => b.TechnicianId == t.Id);
                double booked = 0d;

                foreach (var b in myBookings)
                {
                    // Only count if inside any slot; multiple slots handled by max overlap across slots
                    var inSlots = mySlots.Sum(s => OverlapHours(b.ScheduledStartUtc, b.ScheduledEndUtc, s.StartUtc, s.EndUtc));
                    booked += inSlots;
                }

                result.Add(new TechnicianUtilizationDto
                {
                    TechnicianId = t.Id,
                    TechnicianName = t.User?.FullName ?? $"Tech #{t.Id}",
                    AvailableHours = Math.Round(available, 2),
                    BookedHours = Math.Round(booked, 2),
                });
            }

            return result.OrderByDescending(r => r.UtilizationPercent).ToList();
        }
    }
}
