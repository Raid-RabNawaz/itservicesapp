using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using Xunit;

namespace ITServicesApp.Tests.Unit.Services
{
    public class AdminDashboardServiceTests
    {
        [Fact]
        public async Task GetStatsAsync_ComputesAggregates()
        {
            var options = InMemoryDb.CreateOptions();
            await using var db = new ApplicationDbContext(options);

            var user = new User { Email = "cust@test", FullName = "Customer", PasswordHash = "x", Role = UserRole.Customer };
            var techUser = new User { Email = "tech@test", FullName = "Tech", PasswordHash = "x", Role = UserRole.Technician };
            await db.Users.AddRangeAsync(user, techUser);
            await db.SaveChangesAsync();

            var category = new ServiceCategory { Name = "General", Description = "General" };
            await db.ServiceCategories.AddAsync(category);
            await db.SaveChangesAsync();

            var issue = new ServiceIssue { ServiceCategoryId = category.Id, Name = "Install", EstimatedDurationMinutes = 60, BasePrice = 120m };
            await db.ServiceIssues.AddAsync(issue);
            await db.SaveChangesAsync();

            var technician = new Technician { UserId = techUser.Id, ServiceCategoryId = category.Id, IsActive = true, User = techUser };
            await db.Technicians.AddAsync(technician);
            await db.SaveChangesAsync();

            var now = DateTime.UtcNow;

            var completed = new Booking
            {
                UserId = user.Id,
                TechnicianId = technician.Id,
                Technician = technician,
                User = user,
                ServiceCategoryId = category.Id,
                ServiceIssueId = issue.Id,
                ServiceIssue = issue,
                ScheduledStartUtc = now.AddDays(-2),
                ScheduledEndUtc = now.AddDays(-2).AddHours(1),
                Status = BookingStatus.Completed
            };

            var upcoming = new Booking
            {
                UserId = user.Id,
                TechnicianId = technician.Id,
                Technician = technician,
                User = user,
                ServiceCategoryId = category.Id,
                ServiceIssueId = issue.Id,
                ServiceIssue = issue,
                ScheduledStartUtc = now.AddDays(2),
                ScheduledEndUtc = now.AddDays(2).AddHours(1),
                Status = BookingStatus.Confirmed
            };

            var cancelled = new Booking
            {
                UserId = user.Id,
                TechnicianId = technician.Id,
                Technician = technician,
                User = user,
                ServiceCategoryId = category.Id,
                ServiceIssueId = issue.Id,
                ServiceIssue = issue,
                ScheduledStartUtc = now.AddDays(1),
                ScheduledEndUtc = now.AddDays(1).AddHours(1),
                Status = BookingStatus.Cancelled
            };

            var pending = new Booking
            {
                UserId = user.Id,
                TechnicianId = technician.Id,
                Technician = technician,
                User = user,
                ServiceCategoryId = category.Id,
                ServiceIssueId = issue.Id,
                ServiceIssue = issue,
                ScheduledStartUtc = now.AddDays(3),
                ScheduledEndUtc = now.AddDays(3).AddHours(1),
                Status = BookingStatus.PendingTechnicianConfirmation
            };

            await db.Bookings.AddRangeAsync(completed, upcoming, cancelled, pending);
            await db.SaveChangesAsync();

            await db.Payments.AddAsync(new Payment
            {
                BookingId = completed.Id,
                Method = PaymentMethod.Card,
                Amount = 200m,
                Currency = "USD",
                Status = "Succeeded",
                CreatedAtUtc = now.AddDays(-2)
            });
            await db.SaveChangesAsync();

            var service = new AdminDashboardService(db);

            var result = await service.GetStatsAsync(CancellationToken.None);

            result.TotalBookings.Should().Be(4);
            result.UpcomingBookings.Should().Be(2);
            result.PendingBookings.Should().Be(1);
            result.CompletedBookings.Should().Be(1);
            result.CancelledBookings.Should().Be(1);
            result.ActiveTechnicians.Should().Be(1);
            result.ActiveCustomers.Should().Be(1);
            result.TotalRevenue.Should().Be(200m);
            result.TechnicianNetRevenue.Should().Be(180m);
        }
    }
}
