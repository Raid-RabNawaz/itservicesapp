using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Persistence;
using ITServicesApp.Persistence.Repositories;
using ITServicesApp.Tests.Unit.TestHelpers;
using Moq;
using Xunit;

namespace ITServicesApp.Tests.Unit.Services
{
    public class MessagingServiceTests
    {
        [Fact]
        public async Task GetOrCreateThreadForBookingAsync_Blocks_User_Not_In_Thread()
        {
            var fixture = await CreateFixtureAsync();
            await using var db = fixture.Db;

            fixture.SetCurrentUser(999, UserRole.Customer);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.GetOrCreateThreadForBookingAsync(fixture.BookingId, CancellationToken.None));
        }

        [Fact]
        public async Task Conversation_UnreadCounts_Are_Tracked_PerParticipant()
        {
            var fixture = await CreateFixtureAsync();
            await using var db = fixture.Db;

            fixture.SetCurrentUser(fixture.CustomerUserId, UserRole.Customer);
            var thread = await fixture.Service.GetOrCreateThreadForBookingAsync(fixture.BookingId, CancellationToken.None);
            thread.UnreadForCustomer.Should().Be(0);
            thread.UnreadForTechnician.Should().Be(0);

            fixture.SetCurrentUser(fixture.TechnicianUserId, UserRole.Technician);
            await fixture.Service.SendAsync(new SendMessageDto { ThreadId = thread.Id, Body = "Hello" }, CancellationToken.None);

            fixture.SetCurrentUser(fixture.CustomerUserId, UserRole.Customer);
            var afterMessage = await fixture.Service.GetOrCreateThreadForBookingAsync(fixture.BookingId, CancellationToken.None);
            afterMessage.UnreadForCustomer.Should().Be(1);
            afterMessage.UnreadForTechnician.Should().Be(0);

            await fixture.Service.MarkThreadReadAsync(thread.Id, CancellationToken.None);

            var afterRead = await fixture.Service.GetOrCreateThreadForBookingAsync(fixture.BookingId, CancellationToken.None);
            afterRead.UnreadForCustomer.Should().Be(0);
            afterRead.UnreadForTechnician.Should().Be(0);
        }

        [Fact]
        public async Task SendAsync_Blocks_User_Not_In_Thread()
        {
            var fixture = await CreateFixtureAsync();
            await using var db = fixture.Db;

            fixture.SetCurrentUser(fixture.CustomerUserId, UserRole.Customer);
            var thread = await fixture.Service.GetOrCreateThreadForBookingAsync(fixture.BookingId, CancellationToken.None);

            fixture.SetCurrentUser(999, UserRole.Customer);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.SendAsync(new SendMessageDto { ThreadId = thread.Id, Body = "Blocked" }, CancellationToken.None));
        }

        private static async Task<(ApplicationDbContext Db, MessagingService Service, Action<int, UserRole> SetCurrentUser, int CustomerUserId, int TechnicianUserId, int BookingId)> CreateFixtureAsync()
        {
            var options = InMemoryDb.CreateOptions();
            var db = new ApplicationDbContext(options);

            var customer = new User { Email = "customer@test.local", FullName = "Customer", PasswordHash = "hash", Role = UserRole.Customer };
            var technicianUser = new User { Email = "tech@test.local", FullName = "Technician", PasswordHash = "hash", Role = UserRole.Technician };

            await db.Users.AddRangeAsync(customer, technicianUser);
            await db.SaveChangesAsync();

            var category = new ServiceCategory { Name = "General", Description = "General" };
            await db.ServiceCategories.AddAsync(category);
            await db.SaveChangesAsync();

            var issue = new ServiceIssue { ServiceCategoryId = category.Id, Name = "Issue", EstimatedDurationMinutes = 60, BasePrice = 100m };
            await db.ServiceIssues.AddAsync(issue);
            await db.SaveChangesAsync();

            var technician = new Technician { UserId = technicianUser.Id, ServiceCategoryId = category.Id, IsActive = true };
            await db.Technicians.AddAsync(technician);
            await db.SaveChangesAsync();

            var booking = new Booking
            {
                UserId = customer.Id,
                TechnicianId = technician.Id,
                ServiceCategoryId = category.Id,
                ServiceIssueId = issue.Id,
                ScheduledStartUtc = DateTime.UtcNow.AddDays(1),
                ScheduledEndUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
                Status = BookingStatus.Confirmed,
                CustomerFullName = customer.FullName,
                CustomerEmail = customer.Email
            };
            await db.Bookings.AddAsync(booking);
            await db.SaveChangesAsync();

            var currentUserId = customer.Id;
            var currentRole = UserRole.Customer;

            var current = new Mock<ICurrentUserService>();
            current.SetupGet(x => x.IsAuthenticated).Returns(true);
            current.SetupGet(x => x.UserIdInt).Returns(() => currentUserId);
            current.SetupGet(x => x.UserId).Returns(() => currentUserId.ToString());
            current.SetupGet(x => x.Role).Returns(() => currentRole.ToString());
            current.SetupGet(x => x.Email).Returns((string?)null);

            var repo = new MessageRepository(db);
            var service = new MessagingService(repo, current.Object, db);

            void SetCurrentUser(int userId, UserRole role)
            {
                currentUserId = userId;
                currentRole = role;
            }

            return (db, service, SetCurrentUser, customer.Id, technicianUser.Id, booking.Id);
        }
    }
}
