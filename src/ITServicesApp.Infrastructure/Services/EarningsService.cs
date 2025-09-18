using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Services
{
    public class EarningsService : IEarningsService
    {
        private readonly IInvoiceRepository _invoices;
        private readonly ISettingsRepository _settings;
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        private const decimal PlatformCommissionFallback = 0.10m; // 10% fee -> 90% net for technicians

        public EarningsService(IInvoiceRepository invoices, ISettingsRepository settings, ApplicationDbContext db, ICurrentUserService current)
        {
            _invoices = invoices;
            _settings = settings;
            _db = db;
            _current = current;
        }

        public async Task<TechnicianEarningsSummaryDto> GetSummaryAsync(int technicianId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
        {
            await EnsureTechnicianAccessAsync(technicianId, ct);

            var settings = await _settings.GetSingletonAsync(ct);
            var commissionRate = settings.TechnicianCommissionRate;
            if (commissionRate <= 0m || commissionRate >= 1m)
            {
                commissionRate = PlatformCommissionFallback;
            }
            else if (Math.Abs(commissionRate - PlatformCommissionFallback) > 0.0001m)
            {
                commissionRate = PlatformCommissionFallback;
            }

            var invoices = await _db.Invoices.AsNoTracking()
                .Where(i => i.IssuedAtUtc >= fromUtc && i.IssuedAtUtc <= toUtc)
                .Join(_db.Bookings.AsNoTracking(), i => i.BookingId, b => b.Id, (i, b) => new { invoice = i, booking = b })
                .Where(x => x.booking.TechnicianId == technicianId && x.booking.Status != BookingStatus.Cancelled)
                .Select(x => x.invoice)
                .ToListAsync(ct);

            var gross = invoices.Sum(i => i.Total);
            var commissionFees = Math.Round(gross * commissionRate, 2, MidpointRounding.AwayFromZero);
            var net = Math.Round(gross - commissionFees, 2, MidpointRounding.AwayFromZero);

            return new TechnicianEarningsSummaryDto
            {
                TechnicianId = technicianId,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Gross = Math.Round(gross, 2, MidpointRounding.AwayFromZero),
                CommissionFees = commissionFees,
                Net = net,
                Currency = settings.Currency
            };
        }

        public async Task<IReadOnlyList<TechnicianPayoutDto>> ListPayoutsAsync(int technicianId, int take, int skip, CancellationToken ct)
        {
            await EnsureTechnicianAccessAsync(technicianId, ct);
            // payouts not implemented yet
            return await Task.FromResult((IReadOnlyList<TechnicianPayoutDto>)new List<TechnicianPayoutDto>());
        }

        private async Task EnsureTechnicianAccessAsync(int technicianId, CancellationToken ct)
        {
            var role = GetRole();
            if (role == UserRole.Admin)
                return;

            var currentUserId = _current.UserIdInt;
            if (currentUserId <= 0)
                throw new UnauthorizedAccessException("Authentication required.");

            var technician = await _db.Technicians.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == technicianId, ct)
                ?? throw new InvalidOperationException("Technician not found.");

            if (role == UserRole.Technician && technician.UserId == currentUserId)
                return;

            throw new UnauthorizedAccessException("You cannot view earnings for this technician.");
        }

        private UserRole GetRole()
        {
            var raw = _current.Role;
            return Enum.TryParse<UserRole>(raw, true, out var parsed) ? parsed : UserRole.Customer;
        }
    }
}
