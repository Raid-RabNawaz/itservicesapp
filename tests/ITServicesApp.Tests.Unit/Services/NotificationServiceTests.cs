using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Xunit;
using FluentAssertions;

using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Persistence;
using Moq;
using ITServicesApp.Application.Interfaces.Notifications;
using System.Collections.Generic;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Tests.Unit.Notifications
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task List_and_mark_read_and_mark_all_read_work_for_current_user()
        {
            // Arrange
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);

            var userId = db.Users.First(u => u.Email == "cust@test").Id;

            var uow = TestUowFactory.Create(db);
            var mapper = AutoMapperTestConfig.Create();
            var current = new Mock<ICurrentUserService>();
            current.SetupGet(c => c.UserIdInt).Returns(userId);

            // Use a real channel list with a mocked channel (transport only)
            var ch = new Mock<INotificationChannel>();
            ch.SetupGet(c => c.Name).Returns("noop");
            ch.Setup(c => c.SendAsync(It.IsAny<NotificationDto>(), It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

            var channels = new[] { ch.Object };

            INotificationService svc = new NotificationService(uow, mapper, channels, current.Object);

            // Act: seed two notifications through the service (persists, then fan-out)
            await svc.NotifyUserAsync(userId, "t1", "m1", CancellationToken.None);
            await svc.NotifyUserAsync(userId, "t2", "m2", CancellationToken.None);

            // Assert: unread count & listing
            (await svc.CountUnreadAsync()).Should().Be(2);

            var list = await svc.ListAsync(null, take: 10, skip: 0);
            list.Should().HaveCount(2);

            // Mark one read
            await svc.MarkReadAsync(list[0].Id, CancellationToken.None);
            (await svc.CountUnreadAsync()).Should().Be(1);

            // Mark all read
            await svc.MarkAllReadAsync(CancellationToken.None);
            (await svc.CountUnreadAsync()).Should().Be(0);

            // Verify fan-out happened twice (once per NotifyUserAsync call)
            ch.Verify(c => c.SendAsync(It.IsAny<NotificationDto>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

    }
}
