using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Interfaces.Notifications;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services.Notifications
{
    public class EmailNotificationChannel : INotificationChannel
    {
        public string Name => "email";
        private readonly IEmailService _email;
        private readonly IUserRepository _users;

        public EmailNotificationChannel(IEmailService email, IUserRepository users)
        {
            _email = email;
            _users = users;
        }

        public async Task SendAsync(NotificationDto notification, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(notification.UserId, ct);
            if (user == null) return;

            // simple HTML body; adjust template as needed
            await _email.SendAsync(user.Email, notification.Title, $"<p>{notification.Message}</p>", ct);
        }
    }
}
