using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Infrastructure.Services.Notifications;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace ITServicesApp.Tests.Unit.Notifications
{
    public class InAppNotificationChannelTests
    {
        [Fact]
        public async Task Sends_notify_on_hub_and_persists_notification()
        {
            // Hub mocks
            var clients = new Mock<IHubClients>();
            var clientProxy = new Mock<IClientProxy>();
            clients.Setup(c => c.User("42")).Returns(clientProxy.Object);

            var hub = new Mock<IHubContext<NotificationHub>>();
            hub.SetupGet(c => c.Clients).Returns(clients.Object);

            // UoW + repo mocks
            var notifRepo = new Mock<INotificationRepository>();
            notifRepo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            var uow = new Mock<IUnitOfWork>();
            uow.SetupGet(u => u.Notifications).Returns(notifRepo.Object);
            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // mapper (you already have a helper)
            var mapper = ITServicesApp.Tests.Unit.TestHelpers.AutoMapperTestConfig.Create();

            // channels: use the real in-app channel (transport only)
            var inApp = new ITServicesApp.Infrastructure.Services.Notifications.InAppNotificationChannel(hub.Object);
            var channels = new[] { inApp };

            // current user not needed for NotifyUserAsync, just mock
            var current = new Mock<ICurrentUserService>();

            // SUT: NotificationService (persist + fan-out)
            var svc = new ITServicesApp.Infrastructure.Services.NotificationService(uow.Object, mapper, channels, current.Object);

            // Act
            var userId = 42;
            await svc.NotifyUserAsync(userId, "T", "M", CancellationToken.None);

            // Assert: hub notified
            clientProxy.Verify(c => c.SendCoreAsync(
                    NotificationHub.InAppClientMethod,
                    It.Is<object[]>(args => args.Length == 1),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Assert: persisted
            notifRepo.Verify(r => r.AddAsync(
                    It.Is<Notification>(n => n.UserId == userId && n.Title == "T" && n.Message == "M"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task Sends_notify_on_hub()
        {
            // Arrange
            var hubContext = new Mock<IHubContext<NotificationHub>>();
            var clients = new Mock<IHubClients>();
            var clientProxy = new Mock<IClientProxy>();
            hubContext.Setup(h => h.Clients).Returns(clients.Object);
            clients.Setup(c => c.User("42")).Returns(clientProxy.Object);

            var channel = new InAppNotificationChannel(hubContext.Object);

            var dto = new NotificationDto { UserId = 42, Title = "T", Message = "M" };

            // Act
            await channel.SendAsync(dto, CancellationToken.None);

            // Assert
            clientProxy.Verify(c =>
                c.SendCoreAsync(NotificationHub.InAppClientMethod,
                    It.Is<object[]>(args => args.Length == 1),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
