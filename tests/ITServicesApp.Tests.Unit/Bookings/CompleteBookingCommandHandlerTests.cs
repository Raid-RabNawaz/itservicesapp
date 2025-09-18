using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using Xunit;
using FluentAssertions;

using ITServicesApp.Application.UseCases.Bookings.Commands.CompleteBooking;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Persistence;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Tests.Unit.Bookings
{
    public class CompleteBookingCommandHandlerTests
    {
        [Fact]
        public async Task Completes_booking_cancels_reminder_and_publishes_completed_event()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);
            var start = DateTime.UtcNow.Date.AddDays(1).AddHours(11);
            var id = await TestData.CreateConfirmedBookingAsync(db, start, reminderJobId: "job-x");

            var uow = TestUowFactory.Create(db);
            var mapper = AutoMapperTestConfig.Create();
            var jobs = new Mock<IBackgroundJobService>();
            var mediator = new Mock<IMediator>();

            var handler = new CompleteBookingCommandHandler(uow, mapper, jobs.Object, mediator.Object);

            var dto = await handler.Handle(new CompleteBookingCommand(id, null), CancellationToken.None);

            dto.Status.Should().Be(BookingStatus.Completed);

            jobs.Verify(j => j.CancelBookingReminderAsync("job-x", It.IsAny<CancellationToken>()), Times.Once);
            mediator.Verify(m => m.Publish(It.IsAny<ITServicesApp.Domain.Events.BookingCompletedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
