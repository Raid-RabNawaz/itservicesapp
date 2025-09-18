using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Interfaces.Notifications;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IEnumerable<INotificationChannel> _channels;
        private readonly ICurrentUserService _current;

        public NotificationService(
            IUnitOfWork uow,
            IMapper mapper,
            IEnumerable<INotificationChannel> channels,
            ICurrentUserService current)
        {
            _uow = uow;
            _mapper = mapper;
            _channels = channels;
            _current = current;
        }

        public async Task NotifyUserAsync(int userId, string title, string message, CancellationToken ct = default)
        {
            var entity = new Notification { UserId = userId, Title = title, Message = message };
            await _uow.Notifications.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            var dto = _mapper.Map<NotificationDto>(entity);
            foreach (var ch in _channels)
            {
                await ch.SendAsync(dto, ct);
            }
        }

        public async Task NotifyUsersAsync(IEnumerable<int> userIds, string title, string message, CancellationToken ct = default)
        {
            // Persist all, then fan-out
            var entities = userIds.Select(id => new Notification { UserId = id, Title = title, Message = message }).ToList();
            foreach (var n in entities)
                await _uow.Notifications.AddAsync(n, ct);

            await _uow.SaveChangesAsync(ct);

            var dtos = entities.Select(_mapper.Map<NotificationDto>).ToList();
            foreach (var dto in dtos)
                foreach (var ch in _channels)
                    await ch.SendAsync(dto, ct);
        }

        public async Task<IReadOnlyList<NotificationDto>> ListAsync(int? userId, int take, int skip, CancellationToken ct = default)
        {
            var uid = userId ?? _current.UserIdInt;
            var list = await _uow.Notifications.ListByUserAsync(uid, take, skip, ct);
            return list.Select(_mapper.Map<NotificationDto>).ToList();
        }

        public Task<int> CountUnreadAsync(CancellationToken ct = default)
            => _uow.Notifications.CountUnreadAsync(_current.UserIdInt, ct);

        public async Task MarkReadAsync(int notificationId, CancellationToken ct = default)
        {
            var n = await _uow.Notifications.GetByIdAsync(notificationId, ct)
                    ?? throw new System.InvalidOperationException("Notification not found.");

            if (!n.IsRead)
            {
                n.IsRead = true;
                n.ReadAtUtc = System.DateTime.UtcNow;
                // No Update(): entity is tracked (FindAsync/GetByIdAsync), just Save.
                await _uow.SaveChangesAsync(ct);
            }
        }

        public async Task MarkAllReadAsync(CancellationToken ct = default)
        {
            var uid = _current.UserIdInt;

            // Source repo returns AsNoTracking; fetch IDs, then load tracked instances by key.
            var unread = await _uow.Notifications.ListUnreadAsync(uid, ct);
            if (unread.Count == 0) return;

            var now = System.DateTime.UtcNow;
            foreach (var item in unread)
            {
                var entity = await _uow.Notifications.GetByIdAsync(item.Id, ct);
                if (entity != null && !entity.IsRead)
                {
                    entity.IsRead = true;
                    entity.ReadAtUtc = now;
                }
            }

            await _uow.SaveChangesAsync(ct);
        }
    }
}
