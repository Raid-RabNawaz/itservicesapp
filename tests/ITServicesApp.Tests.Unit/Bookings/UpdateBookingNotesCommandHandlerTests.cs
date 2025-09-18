using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using Xunit;
using FluentAssertions;

using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.UseCases.Bookings.Commands.UpdateBookingNotes;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Persistence;

namespace ITServicesApp.Tests.Unit.Bookings
{
    public class UpdateBookingNotesCommandHandlerTests
    {
        [Fact]
        public async Task Updates_notes_and_publishes_update_event()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);
            var id = await TestData.CreateConfirmedBookingAsync(db, DateTime.UtcNow.Date.AddDays(2).AddHours(9));

            var uow = TestUowFactory.Create(db);
            var mapper = AutoMapperTestConfig.Create();
            var mediator = new Mock<IMediator>();

            var handler = new UpdateBookingNotesCommandHandler(uow, mapper, mediator.Object);
            var result = await handler.Handle(new UpdateBookingNotesCommand(new UpdateBookingNotesDto { BookingId = id, Notes = "updated" }), CancellationToken.None);

            result.Notes.Should().Be("updated");
            mediator.Verify(m => m.Publish(It.IsAny<ITServicesApp.Domain.Events.BookingUpdatedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
