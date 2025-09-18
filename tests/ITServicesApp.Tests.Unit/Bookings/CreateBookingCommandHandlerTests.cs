using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using Xunit;
using FluentAssertions;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateBooking;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Persistence;

namespace ITServicesApp.Tests.Unit.Bookings
{
    public class CreateBookingCommandHandlerTests
    {
        [Fact]
        public async Task Creates_booking_schedules_reminder_and_publishes_event()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);
            var uow = TestUowFactory.Create(db);
            var mapper = AutoMapperTestConfig.Create();
            var clock = new FakeDateTimeProvider { UtcNow = DateTime.UtcNow.Date.AddHours(8) };

            var jobs = new Mock<IBackgroundJobService>();
            jobs.Setup(j => j.ScheduleBookingReminderAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("job-123");

            var mediator = new Mock<IMediator>();

            var handler = new CreateBookingCommandHandler(uow, mapper, clock, jobs.Object, mediator.Object);

            var tech = db.Technicians.First();
            var cat = db.ServiceCategories.First();
            var iss = db.ServiceIssues.First(i => i.ServiceCategoryId == cat.Id);
            var user = db.Users.First(u => u.Role == UserRole.Customer);

            var start = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            var dto = new CreateBookingDto
            {
                UserId = user.Id,
                TechnicianId = tech.Id,
                ServiceCategoryId = cat.Id,
                ServiceIssueId = iss.Id,
                Start = start,
                End = start.AddHours(1),
                Address = new BookingAddressDto { Line1 = "123 Test St", City = "City", State = "State", PostalCode = "00000", Country = "US" },
                Notes = "N",
                PreferredPaymentMethod = PaymentMethod.Cash,
                Items = { new CreateBookingItemDto { ServiceIssueId = iss.Id, Quantity = 1, UnitPrice = iss.BasePrice } }
            };

            var res = await handler.Handle(new CreateBookingCommand(dto, "idem-1"), CancellationToken.None);

            res.Should().NotBeNull();
            res.Id.Should().BeGreaterThan(0);

            var booking = await db.Bookings.FindAsync(res.Id);
            booking!.ReminderJobId.Should().Be("job-123");
            booking.Status.Should().Be(BookingStatus.PendingTechnicianConfirmation);
            booking.EstimatedTotal.Should().Be(iss.BasePrice);

            jobs.Verify(j => j.ScheduleBookingReminderAsync(res.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
            mediator.Verify(m => m.Publish(It.IsAny<ITServicesApp.Domain.Events.BookingCreatedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Idempotency_returns_existing_booking()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);
            var uow = TestUowFactory.Create(db);
            var mapper = AutoMapperTestConfig.Create();
            var clock = new FakeDateTimeProvider { UtcNow = DateTime.UtcNow.Date.AddHours(8) };

            var jobs = new Mock<IBackgroundJobService>();
            var mediator = new Mock<IMediator>();
            var handler = new CreateBookingCommandHandler(uow, mapper, clock, jobs.Object, mediator.Object);

            var tech = db.Technicians.First();
            var cat = db.ServiceCategories.First();
            var iss = db.ServiceIssues.First(i => i.ServiceCategoryId == cat.Id);
            var user = db.Users.First(u => u.Role == UserRole.Customer);

            var start = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            // seed an existing same-idempotency booking
            var existing = new ITServicesApp.Domain.Entities.Booking
            {
                UserId = user.Id,
                TechnicianId = tech.Id,
                ServiceCategoryId = cat.Id,
                ServiceIssueId = iss.Id,
                ScheduledStartUtc = start,
                ScheduledEndUtc = start.AddHours(1),
                Status = BookingStatus.Confirmed,
                ClientRequestId = "idem-xyz",
                CustomerFullName = user.FullName,
                CustomerEmail = user.Email,
                PreferredPaymentMethod = PaymentMethod.Cash
            };
            existing.Items.Add(new ITServicesApp.Domain.Entities.BookingItem
            {
                ServiceIssueId = iss.Id,
                ServiceName = iss.Name,
                UnitPrice = iss.BasePrice,
                Quantity = 1,
                LineTotal = iss.BasePrice
            });
            existing.EstimatedTotal = iss.BasePrice;
            existing.FinalTotal = iss.BasePrice;
            db.Bookings.Add(existing);
            await db.SaveChangesAsync();

            var dto = new CreateBookingDto
            {
                UserId = user.Id,
                TechnicianId = tech.Id,
                ServiceCategoryId = cat.Id,
                ServiceIssueId = iss.Id,
                Start = start,
                End = start.AddHours(1),
                PreferredPaymentMethod = PaymentMethod.Cash,
                Items = { new CreateBookingItemDto { ServiceIssueId = iss.Id, Quantity = 1, UnitPrice = iss.BasePrice } }
            };

            var res = await handler.Handle(new CreateBookingCommand(dto, "idem-xyz"), CancellationToken.None);
            res.Id.Should().Be(existing.Id);

            jobs.Verify(j => j.ScheduleBookingReminderAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            mediator.Verify(m => m.Publish(It.IsAny<ITServicesApp.Domain.Events.BookingCreatedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
