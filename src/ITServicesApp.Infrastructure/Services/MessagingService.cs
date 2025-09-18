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

namespace ITServicesApp.Infrastructure.Services
{
    public class MessagingService : IMessageService
    {
        private readonly IMessageRepository _repo;
        private readonly ICurrentUserService _me;
        private readonly ApplicationDbContext _db;

        public MessagingService(IMessageRepository repo, ICurrentUserService me, ApplicationDbContext db)
        {
            _repo = repo;
            _me = me;
            _db = db;
        }

        public async Task<MessageThreadDto> GetOrCreateThreadForBookingAsync(int bookingId, CancellationToken ct)
        {
            var booking = await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId, ct)
                ?? throw new KeyNotFoundException("Booking not found.");

            var technicianUserId = await _db.Technicians.AsNoTracking()
                .Where(t => t.Id == booking.TechnicianId)
                .Select(t => t.UserId)
                .FirstOrDefaultAsync(ct);

            if (technicianUserId == 0)
            {
                throw new InvalidOperationException("Booking does not have an assigned technician user.");
            }

            EnsureCurrentUserCanAccess(booking.UserId, technicianUserId);

            var thread = await _repo.GetThreadByBookingAsync(bookingId, ct);
            if (thread == null)
            {
                var threadEntity = new MessageThread
                {
                    BookingId = bookingId,
                    CustomerId = booking.UserId,
                    TechnicianId = technicianUserId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                thread = await _repo.AddThreadAsync(threadEntity, ct);
            }

            var (unreadForCustomer, unreadForTechnician) = await CalculateUnreadCountsAsync(thread.Id, thread.CustomerId, thread.TechnicianId, ct);

            return new MessageThreadDto
            {
                Id = thread.Id,
                BookingId = thread.BookingId,
                CustomerId = thread.CustomerId,
                TechnicianId = thread.TechnicianId,
                CreatedAtUtc = thread.CreatedAtUtc,
                UnreadForCustomer = unreadForCustomer,
                UnreadForTechnician = unreadForTechnician
            };
        }

        public async Task<IReadOnlyList<MessageDto>> ListAsync(int threadId, int take, int skip, CancellationToken ct)
        {
            await GetThreadAsync(threadId, ct); // ensures access

            var list = await _repo.ListMessagesAsync(threadId, take, skip, ct);
            return list.Select(m => new MessageDto
            {
                Id = m.Id,
                ThreadId = m.ThreadId,
                SenderUserId = m.SenderUserId,
                Body = m.Body,
                SentAtUtc = m.SentAtUtc,
                IsRead = m.IsRead
            }).ToList();
        }

        public async Task<MessageDto> SendAsync(SendMessageDto dto, CancellationToken ct)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Body))
                throw new ArgumentException("Message body is required.", nameof(dto.Body));

            var thread = await GetThreadAsync(dto.ThreadId, ct);
            var body = dto.Body.Trim();

            var msg = await _repo.AddMessageAsync(new Message
            {
                ThreadId = thread.Id,
                SenderUserId = _me.UserIdInt,
                Body = body,
                SentAtUtc = DateTime.UtcNow,
                IsRead = false
            }, ct);

            return new MessageDto
            {
                Id = msg.Id,
                ThreadId = msg.ThreadId,
                SenderUserId = msg.SenderUserId,
                Body = msg.Body,
                SentAtUtc = msg.SentAtUtc,
                IsRead = msg.IsRead
            };
        }

        public async Task MarkThreadReadAsync(int threadId, CancellationToken ct)
        {
            var thread = await GetThreadAsync(threadId, ct);
            var readerId = _me.UserIdInt;

            var messagesToMark = await _db.Messages
                .Where(m => m.ThreadId == thread.Id && m.SenderUserId != readerId && !m.IsRead)
                .ToListAsync(ct);

            if (messagesToMark.Count == 0) return;

            foreach (var message in messagesToMark)
            {
                message.IsRead = true;
            }

            await _db.SaveChangesAsync(ct);
        }

        private async Task<MessageThread> GetThreadAsync(int threadId, CancellationToken ct)
        {
            var thread = await _db.MessageThreads.FirstOrDefaultAsync(t => t.Id == threadId, ct)
                ?? throw new KeyNotFoundException("Message thread not found.");

            EnsureCurrentUserCanAccess(thread.CustomerId, thread.TechnicianId);
            return thread;
        }

        private void EnsureCurrentUserCanAccess(int customerUserId, int technicianUserId)
        {
            var currentUserId = _me.UserIdInt;
            if (currentUserId <= 0)
            {
                throw new UnauthorizedAccessException("Authentication required.");
            }

            if (IsAdmin()) return;

            if (currentUserId != customerUserId && currentUserId != technicianUserId)
            {
                throw new UnauthorizedAccessException("You do not have access to this conversation.");
            }
        }

        private bool IsAdmin()
        {
            if (string.IsNullOrWhiteSpace(_me.Role)) return false;
            return Enum.TryParse<UserRole>(_me.Role, true, out var parsed) && parsed == UserRole.Admin;
        }

        private async Task<(int customer, int technician)> CalculateUnreadCountsAsync(int threadId, int customerUserId, int technicianUserId, CancellationToken ct)
        {
            var unreadSenderIds = await _db.Messages
                .Where(m => m.ThreadId == threadId && !m.IsRead)
                .Select(m => m.SenderUserId)
                .ToListAsync(ct);

            var unreadForCustomer = unreadSenderIds.Count(senderId => senderId != customerUserId);
            var unreadForTechnician = technicianUserId > 0
                ? unreadSenderIds.Count(senderId => senderId != technicianUserId)
                : 0;

            return (unreadForCustomer, unreadForTechnician);
        }
    }
}
