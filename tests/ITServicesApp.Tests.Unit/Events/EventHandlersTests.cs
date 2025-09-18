using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;

using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.UseCases.Bookings.Events;
using ITServicesApp.Domain.Events;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;

namespace ITServicesApp.Tests.Unit.Events
{
    public class EventHandlersTests
    {
        [Fact]
        public async Task BookingCompletedDomainEventHandler_notifies_user()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);
            var id = await TestData.CreateConfirmedBookingAsync(db, DateTime.UtcNow.Date.AddDays(1).AddHours(10));

            var uow = TestUowFactory.Create(db);
            var notifications = new Mock<INotificationService>();
            var handler = new BookingCompletedDomainEventHandler(uow, notifications.Object);

            await handler.Handle(new BookingCompletedDomainEvent(id), CancellationToken.None);

            notifications.Verify(n => n.NotifyUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
