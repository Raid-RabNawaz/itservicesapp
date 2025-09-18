using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;
using Moq;
using Xunit;

namespace ITServicesApp.Tests.Unit.Services
{
    public class CustomerDashboardServiceTests
    {
        [Fact]
        public async Task GetAsync_ReturnsUpcomingAndRecent()
        {
            var fixture = await CustomerFixture.CreateAsync();
            await using var _ = fixture.Db;

            fixture.SetCurrentUser(fixture.CustomerId, UserRole.Customer);

            var dto = await fixture.Service.GetAsync(fixture.CustomerId, CancellationToken.None);

            dto.UpcomingBookings.Should().HaveCount(2);
            dto.RecentBookings.Should().HaveCount(1);
            dto.ActiveRequests.Should().Be(1);
            dto.TotalSpentLast90Days.Should().Be(150m);
        }

        [Fact]
        public async Task GetAsync_BlocksOtherCustomers()
        {
            var fixture = await CustomerFixture.CreateAsync();
            await using var _ = fixture.Db;

            fixture.SetCurrentUser(999, UserRole.Customer);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.GetAsync(fixture.CustomerId, CancellationToken.None));
        }

        [Fact]
        public async Task GetAsync_AllowsAdmin()
        {
            var fixture = await CustomerFixture.CreateAsync();
            await using var _ = fixture.Db;

            fixture.SetCurrentUser(123, UserRole.Admin);

            var dto = await fixture.Service.GetAsync(fixture.CustomerId, CancellationToken.None);
            dto.ActiveRequests.Should().Be(1);
        }

        private sealed class CustomerFixture
        {
            public ApplicationDbContext Db { get; }
            public CustomerDashboardService Service { get; }
            public int CustomerId { get; }
            private readonly Mock<ICurrentUserService> _current;

            private CustomerFixture(ApplicationDbContext db, CustomerDashboardService service, int customerId, Mock<ICurrentUserService> current)
            {
                Db = db;
                Service = service;
                CustomerId = customerId;
                _current = current;
            }

            public void SetCurrentUser(int userId, UserRole role)
            {
                _current.SetupGet(x => x.UserIdInt).Returns(userId);
                _current.SetupGet(x => x.UserId).Returns(() => userId.ToString());
                _current.SetupGet(x => x.Role).Returns(() => role.ToString());
                _current.SetupGet(x => x.IsAuthenticated).Returns(true);
            }

            public static async Task<CustomerFixture> CreateAsync()
            {
                var options = InMemoryDb.CreateOptions();
                var db = new ApplicationDbContext(options);

                var customer = new User { Email = "customer@test", FullName = "Customer", PasswordHash = "hash", Role = UserRole.Customer };
                var technicianUser = new User { Email = "tech@test", FullName = "Technician", PasswordHash = "hash", Role = UserRole.Technician };

                await db.Users.AddRangeAsync(customer, technicianUser);
                await db.SaveChangesAsync();

                var category = new ServiceCategory { Name = "General", Description = "General" };
                await db.ServiceCategories.AddAsync(category);
                await db.SaveChangesAsync();

                var issue = new ServiceIssue { ServiceCategoryId = category.Id, Name = "Install", EstimatedDurationMinutes = 60, BasePrice = 150m };
                await db.ServiceIssues.AddAsync(issue);
                await db.SaveChangesAsync();

                var technician = new Technician { UserId = technicianUser.Id, ServiceCategoryId = category.Id, IsActive = true, User = technicianUser };
                await db.Technicians.AddAsync(technician);
                await db.SaveChangesAsync();

                var now = DateTime.UtcNow;

                var upcoming = new Booking
                {
                    UserId = customer.Id,
                    TechnicianId = technician.Id,
                    Technician = technician,
                    User = customer,
                    ServiceCategoryId = category.Id,
                    ServiceIssueId = issue.Id,
                    ServiceIssue = issue,
                    ScheduledStartUtc = now.AddDays(3),
                    ScheduledEndUtc = now.AddDays(3).AddHours(2),
                    Status = BookingStatus.Confirmed,
                    EstimatedTotal = 120m
                };

                var recent = new Booking
                {
                    UserId = customer.Id,
                    TechnicianId = technician.Id,
                    Technician = technician,
                    User = customer,
                    ServiceCategoryId = category.Id,
                    ServiceIssueId = issue.Id,
                    ServiceIssue = issue,
                    ScheduledStartUtc = now.AddDays(-10),
                    ScheduledEndUtc = now.AddDays(-10).AddHours(1),
                    Status = BookingStatus.Completed,
                    FinalTotal = 150m
                };

                var active = new Booking
                {
                    UserId = customer.Id,
                    TechnicianId = technician.Id,
                    Technician = technician,
                    User = customer,
                    ServiceCategoryId = category.Id,
                    ServiceIssueId = issue.Id,
                    ServiceIssue = issue,
                    ScheduledStartUtc = now.AddDays(1),
                    ScheduledEndUtc = now.AddDays(1).AddHours(1),
                    Status = BookingStatus.PendingCustomerConfirmation,
                    EstimatedTotal = 80m
                };

                await db.Bookings.AddRangeAsync(upcoming, recent, active);
                await db.SaveChangesAsync();

                await db.Invoices.AddAsync(new Invoice
                {
                    BookingId = recent.Id,
                    IssuedAtUtc = now.AddDays(-9),
                    Number = "INV-1",
                    Subtotal = 130m,
                    Tax = 20m,
                    Total = 150m,
                    Currency = "USD"
                });
                await db.SaveChangesAsync();

                var current = new Mock<ICurrentUserService>();
                current.SetupGet(x => x.IsAuthenticated).Returns(true);

                var service = new CustomerDashboardService(db, current.Object);

                return new CustomerFixture(db, service, customer.Id, current);
            }
        }
    }
}

