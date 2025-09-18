using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateBooking;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateGuestBooking;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Persistence;
using ITServicesApp.Infrastructure.Security;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Application.Options;
using Microsoft.Extensions.Options;

namespace ITServicesApp.Tests.Unit.Bookings
{
    public class CreateGuestBookingCommandHandlerTests
    {
        [Fact]
        public async Task Creates_user_and_booking_for_guest()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);

            var mapper = AutoMapperTestConfig.Create();
            var uow = TestUowFactory.Create(db);
            var clock = new FakeDateTimeProvider { UtcNow = DateTime.UtcNow.Date.AddHours(8) };

            var email = new Mock<IEmailService>();
            email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
            var jwt = new Mock<IJwtTokenService>();
            var current = new Mock<ICurrentUserService>();
            var passwordHasher = new PasswordHasher();

            var reset = new Mock<IPasswordResetService>();
            reset.Setup(r => r.GenerateAndStoreTokenAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync("token-guest");
            var frontend = Options.Create(new FrontendOptions { BaseUrl = "https://app.test", FirstLoginPath = "/first-login" });

            var userService = new UserService(uow, mapper, passwordHasher, email.Object, jwt.Object, current.Object, db, reset.Object, frontend);


            var backgroundJobs = new Mock<IBackgroundJobService>();
            backgroundJobs.Setup(j => j.ScheduleBookingReminderAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync("job-guest");

            var mediator = new Mock<IMediator>();
            var bookingHandler = new CreateBookingCommandHandler(uow, mapper, clock, backgroundJobs.Object, mediator.Object);
            mediator.Setup(m => m.Send(It.IsAny<CreateBookingCommand>(), It.IsAny<CancellationToken>()))
                    .Returns<CreateBookingCommand, CancellationToken>((cmd, ct) => bookingHandler.Handle(cmd, ct));
            mediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var handler = new CreateGuestBookingCommandHandler(userService, mediator.Object);

            var tech = db.Technicians.First();
            var issue = db.ServiceIssues.First();

            var start = DateTime.UtcNow.Date.AddDays(2).AddHours(10);
            db.TechnicianSlots.Add(new TechnicianSlot
            {
                TechnicianId = tech.Id,
                StartUtc = start.AddHours(-2),
                EndUtc = start.AddHours(2)
            });
            await db.SaveChangesAsync();

            var guestRequest = new GuestBookingRequestDto
            {
                FullName = "Guest User",
                Email = "guest@example.com",
                Phone = "1234567890",
                TechnicianId = tech.Id,
                ServiceCategoryId = issue.ServiceCategoryId,
                ServiceIssueId = issue.Id,
                Start = start,
                End = start.AddHours(1),
                PreferredPaymentMethod = PaymentMethod.Card,
                Notes = "Door code 123",
                Address = new BookingAddressDto
                {
                    Line1 = "456 Guest St",
                    City = "Guest City",
                    State = "GC",
                    PostalCode = "11111",
                    Country = "US"
                },
                Items = { new CreateBookingItemDto { ServiceIssueId = issue.Id, Quantity = 1, UnitPrice = issue.BasePrice } },
                ClientRequestId = "guest-req-1"
            };

            var result = await handler.Handle(new CreateGuestBookingCommand(guestRequest), CancellationToken.None);

            result.RequiresLogin.Should().BeFalse();
            result.Booking.Should().NotBeNull();
            result.Booking!.CustomerEmail.Should().Be("guest@example.com");
            result.Booking.Items.Should().HaveCount(1);

            db.Users.Any(u => u.Email == "guest@example.com").Should().BeTrue();
            db.Bookings.Any(b => b.CustomerEmail == "guest@example.com").Should().BeTrue();

            backgroundJobs.Verify(j => j.ScheduleBookingReminderAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Returns_conflict_when_user_exists()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);

            var mapper = AutoMapperTestConfig.Create();
            var uow = TestUowFactory.Create(db);

            var email = new Mock<IEmailService>();
            email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
            var jwt = new Mock<IJwtTokenService>();
            var current = new Mock<ICurrentUserService>();
            var passwordHasher = new PasswordHasher();
            var reset = new Mock<IPasswordResetService>();
            reset.Setup(r => r.GenerateAndStoreTokenAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync("token-guest-conflict");
            var frontend = Options.Create(new FrontendOptions { BaseUrl = "https://app.test", FirstLoginPath = "/first-login" });

            var userService = new UserService(uow, mapper, passwordHasher, email.Object, jwt.Object, current.Object, db, reset.Object, frontend);

            var mediator = new Mock<IMediator>();
            mediator.Setup(m => m.Send(It.IsAny<CreateBookingCommand>(), It.IsAny<CancellationToken>()))
                    .Throws(new InvalidOperationException("Should not create booking"));

            var handler = new CreateGuestBookingCommandHandler(userService, mediator.Object);

            var tech = db.Technicians.First();
            var issue = db.ServiceIssues.First();
            var existingUser = db.Users.First(u => u.Role == UserRole.Customer);

            var request = new GuestBookingRequestDto
            {
                FullName = "Existing User",
                Email = existingUser.Email,
                TechnicianId = tech.Id,
                ServiceCategoryId = issue.ServiceCategoryId,
                ServiceIssueId = issue.Id,
                Start = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
                End = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
                PreferredPaymentMethod = PaymentMethod.Cash,
                Address = new BookingAddressDto { Line1 = "123", City = "City", PostalCode = "00000", Country = "US" }
            };

            var result = await handler.Handle(new CreateGuestBookingCommand(request), CancellationToken.None);

            result.RequiresLogin.Should().BeTrue();
            result.ExistingUserId.Should().Be(existingUser.Id);
            result.Booking.Should().BeNull();
        }
    }
}







