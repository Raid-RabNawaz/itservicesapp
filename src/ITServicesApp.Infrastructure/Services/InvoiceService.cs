using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace ITServicesApp.Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repo;
        private readonly ICurrentUserService _current;
        private readonly ApplicationDbContext _db;

        public InvoiceService(IInvoiceRepository repo, ICurrentUserService current, ApplicationDbContext db)
        {
            _repo = repo;
            _current = current;
            _db = db;
        }

        public async Task<IReadOnlyList<InvoiceDto>> ListMyInvoicesAsync(int take, int skip, CancellationToken ct)
        {
            var role = GetRole();
            var query = from invoice in _db.Invoices.AsNoTracking()
                        join booking in _db.Bookings.AsNoTracking() on invoice.BookingId equals booking.Id
                        select new { invoice, booking };

            if (role != UserRole.Admin)
            {
                var currentUserId = RequireUserId();
                query = query.Where(x => x.booking.UserId == currentUserId);
            }

            var list = await query
                .OrderByDescending(x => x.invoice.IssuedAtUtc)
                .Skip(skip)
                .Take(take)
                .Select(x => x.invoice)
                .ToListAsync(ct);

            return list.Select(ToDto).ToList();
        }

        public async Task<InvoiceDto?> GetByBookingAsync(int bookingId, CancellationToken ct)
        {
            await EnsureBookingAccessAsync(bookingId, ct);
            var inv = await _repo.GetByBookingAsync(bookingId, ct);
            return inv is null ? null : ToDto(inv);
        }

        public async Task<byte[]> RenderPdfAsync(int invoiceId, CancellationToken ct)
        {
            var inv = await _repo.GetByIdAsync(invoiceId, ct) ?? throw new KeyNotFoundException();
            await EnsureBookingAccessAsync(inv.BookingId, ct);

            return Document.Create(c =>
                c.Page(p =>
                {
                    p.Margin(40);
                    p.Header().Text($"Invoice {inv.Number}").FontSize(20).Bold();
                    p.Content().Text($"Total: {inv.Total} {inv.Currency}");
                    p.Footer().AlignRight().Text("Thank you");
                })).GeneratePdf();
        }

        private async Task EnsureBookingAccessAsync(int bookingId, CancellationToken ct)
        {
            var booking = await _db.Bookings.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
                ?? throw new InvalidOperationException("Booking not found.");

            var currentUserId = _current.UserIdInt;
            if (currentUserId <= 0)
                throw new UnauthorizedAccessException("Authentication required.");

            var role = GetRole();
            if (role == UserRole.Admin)
                return;

            if (booking.UserId == currentUserId)
                return;

            if (role == UserRole.Technician)
            {
                var technician = await _db.Technicians.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == booking.TechnicianId, ct);
                if (technician != null && technician.UserId == currentUserId)
                    return;
            }

            throw new UnauthorizedAccessException("You cannot access this invoice.");
        }

        private int RequireUserId()
        {
            if (!_current.IsAuthenticated || _current.UserIdInt <= 0)
                throw new UnauthorizedAccessException("Authentication required.");
            return _current.UserIdInt;
        }

        private UserRole GetRole()
        {
            var raw = _current.Role;
            return Enum.TryParse<UserRole>(raw, true, out var parsed) ? parsed : UserRole.Customer;
        }

        private static InvoiceDto ToDto(Invoice x) => new InvoiceDto
        {
            Id = x.Id,
            BookingId = x.BookingId,
            Number = x.Number,
            IssuedAtUtc = x.IssuedAtUtc,
            Subtotal = x.Subtotal,
            Tax = x.Tax,
            Total = x.Total,
            Currency = x.Currency,
            Status = x.Status
        };
    }
}
