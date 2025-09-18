using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using Xunit;
using FluentAssertions;

using ITServicesApp.Application.UseCases.Bookings.Commands.CancelBooking;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Persistence;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Tests.Unit.Bookings
{
    public class CancelBookingCommandHandlerTests
    {
        [Fact]
        public async Task Cancels_booking_and_cancels_reminder_and_publishes_event()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);
            var start = DateTime.UtcNow.Date.AddDays(2).AddHours(11);
            var id = await TestData.CreateConfirmedBookingAsync(db, start, reminderJobId: "job-1");

            var uow = TestUowFactory.Create(db);
            var mapper = AutoMapperTestConfig.Create();
            var clock = new FakeDateTimeProvider { UtcNow = DateTime.UtcNow.Date.AddHours(8) };

            var jobs = new Mock<IBackgroundJobService>();
            var mediator = new Mock<IMediator>();

            var handler = new CancelBookingCommandHandler(uow, mapper, clock, jobs.Object, mediator.Object);

            var dto = await handler.Handle(new CancelBookingCommand(id), CancellationToken.None);

            dto.Status.Should().Be(BookingStatus.Cancelled);

            jobs.Verify(j => j.CancelBookingReminderAsync("job-1", It.IsAny<CancellationToken>()), Times.Once);
            mediator.Verify(m => m.Publish(It.IsAny<ITServicesApp.Domain.Events.BookingCancelledDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
