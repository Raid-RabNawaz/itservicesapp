using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.UseCases.Bookings.Commands.RescheduleBooking;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;

namespace ITServicesApp.Tests.Unit.Bookings
{
    public class RescheduleBookingCommandHandlerTests
    {
        private static (ApplicationDbContext db, IMapper mapper, FakeDateTimeProvider clock) Arrange()
        {
            var opts = InMemoryDb.CreateOptions();
            var db = new ApplicationDbContext(opts);
            var mapper = AutoMapperTestConfig.Create(); // no global Assert
            var clock = new FakeDateTimeProvider { UtcNow = DateTime.UtcNow.Date.AddHours(8) };
            return (db, mapper, clock);
        }

        [Theory]
        // dayOffset, ensureExtraSlot
        [InlineData(1, false)] // within seeded slot day (tomorrow)
        [InlineData(2, true)]  // two days ahead, create a slot first
        public async Task Reschedules_ok_when_slot_exists(int dayOffset, bool ensureExtraSlot)
        {
            var (db, mapper, clock) = Arrange();
            await TestData.SeedBasicAsync(db); // seeds a slot for tomorrow 09–17

            var uow = TestUowFactory.Create(db);

            // Create the original booking for the target day
            var start = DateTime.UtcNow.Date.AddDays(dayOffset).AddHours(10);
            var bookingId = await TestData.CreateConfirmedBookingAsync(db, start, reminderJobId: "job-old");

            // Make sure there is a slot on that day if needed
            if (ensureExtraSlot)
            {
                var techId = db.Technicians.Select(t => t.Id).First();
                await TestData.EnsureSlotForAsync(db, techId, start.Date); // add 09–17 slot for that day
            }

            var jobs = new Mock<IBackgroundJobService>();
            jobs.Setup(j => j.ScheduleBookingReminderAsync(bookingId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("job-new");

            var mediator = new Mock<IMediator>();

            var handler = new RescheduleBookingCommandHandler(uow, mapper, clock, jobs.Object, mediator.Object);

            var newStart = start.AddHours(2);
            var result = await handler.Handle(new RescheduleBookingCommand(bookingId, newStart, newStart.AddHours(1), null), CancellationToken.None);

            result.ScheduledStartUtc.Should().Be(newStart);

            jobs.Verify(j => j.CancelBookingReminderAsync("job-old", It.IsAny<CancellationToken>()), Times.Once);
            jobs.Verify(j => j.ScheduleBookingReminderAsync(bookingId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
            mediator.Verify(m => m.Publish(It.IsAny<ITServicesApp.Domain.Events.BookingUpdatedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            await db.DisposeAsync();
        }

        [Fact]
        public async Task Reschedule_fails_when_no_slot_covers_new_time()
        {
            var (db, mapper, clock) = Arrange();
            await TestData.SeedBasicAsync(db); // only seeds tomorrow 09–17

            var uow = TestUowFactory.Create(db);
            var start = DateTime.UtcNow.Date.AddDays(2).AddHours(10); // two days ahead (no slot seeded)
            var bookingId = await TestData.CreateConfirmedBookingAsync(db, start, reminderJobId: "job-x");

            var jobs = new Mock<IBackgroundJobService>();
            var mediator = new Mock<IMediator>();
            var handler = new RescheduleBookingCommandHandler(uow, mapper, clock, jobs.Object, mediator.Object);

            var newStart = start.AddHours(1);

            var act = async () => await handler.Handle(new RescheduleBookingCommand(bookingId, newStart, newStart.AddHours(1), null), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Technician has no slot covering the new time.");

            jobs.Verify(j => j.CancelBookingReminderAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            mediator.Verify(m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);

            await db.DisposeAsync();
        }
    }
}
