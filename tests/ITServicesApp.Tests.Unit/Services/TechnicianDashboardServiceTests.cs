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
    public class TechnicianDashboardServiceTests
    {
        [Fact]
        public async Task GetAsync_ReturnsDashboardWithUpcomingAndRecent()
        {
            var fixture = await TechnicianFixture.CreateAsync();
            await using var _ = fixture.Db;

            fixture.SetCurrentUser(fixture.TechnicianUserId, UserRole.Technician);

            var summary = new TechnicianEarningsSummaryDto
            {
                TechnicianId = fixture.TechnicianId,
                FromUtc = DateTime.UtcNow.AddDays(-30),
                ToUtc = DateTime.UtcNow,
                Gross = 200m,
                CommissionFees = 20m,
                Net = 180m,
                Currency = "USD"
            };
            fixture.EarningsService.Setup(e => e.GetSummaryAsync(fixture.TechnicianId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(summary);

            var result = await fixture.Service.GetAsync(fixture.TechnicianId, null, null, CancellationToken.None);

            result.Earnings.Net.Should().Be(180m);
            result.UpcomingBookings.Should().HaveCount(2);
            result.RecentBookings.Should().HaveCount(1);
            result.PendingConfirmationCount.Should().Be(1);
            result.CompletedLast30Days.Should().Be(1);
        }

        [Fact]
        public async Task GetAsync_BlocksAccessForOtherUsers()
        {
            var fixture = await TechnicianFixture.CreateAsync();
            await using var _ = fixture.Db;

            fixture.SetCurrentUser(999, UserRole.Customer);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.GetAsync(fixture.TechnicianId, null, null, CancellationToken.None));
        }

        private sealed class TechnicianFixture
        {
            public ApplicationDbContext Db { get; }
            public TechnicianDashboardService Service { get; }
            public Mock<IEarningsService> EarningsService { get; }
            public int TechnicianId { get; }
            public int TechnicianUserId { get; }
            public int CustomerUserId { get; }
            private readonly Mock<ICurrentUserService> _current;

            private TechnicianFixture(ApplicationDbContext db, TechnicianDashboardService service, Mock<IEarningsService> earnings, Mock<ICurrentUserService> current, int technicianId, int technicianUserId, int customerUserId)
            {
                Db = db;
                Service = service;
                EarningsService = earnings;
                TechnicianId = technicianId;
                TechnicianUserId = technicianUserId;
                CustomerUserId = customerUserId;
                _current = current;
            }

            public void SetCurrentUser(int userId, UserRole role)
            {
                _current.SetupGet(x => x.UserIdInt).Returns(userId);
                _current.SetupGet(x => x.UserId).Returns(() => userId.ToString());
                _current.SetupGet(x => x.Role).Returns(() => role.ToString());
                _current.SetupGet(x => x.IsAuthenticated).Returns(true);
            }

            public static async Task<TechnicianFixture> CreateAsync()
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
                    ScheduledStartUtc = now.AddDays(2),
                    ScheduledEndUtc = now.AddDays(2).AddHours(2),
                    Status = BookingStatus.Confirmed,
                    EstimatedTotal = 200m
                };

                var completed = new Booking
                {
                    UserId = customer.Id,
                    TechnicianId = technician.Id,
                    Technician = technician,
                    User = customer,
                    ServiceCategoryId = category.Id,
                    ServiceIssueId = issue.Id,
                    ServiceIssue = issue,
                    ScheduledStartUtc = now.AddDays(-5),
                    ScheduledEndUtc = now.AddDays(-5).AddHours(2),
                    Status = BookingStatus.Completed,
                    FinalTotal = 180m
                };

                var pending = new Booking
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
                    Status = BookingStatus.PendingTechnicianConfirmation,
                    EstimatedTotal = 120m
                };

                await db.Bookings.AddRangeAsync(upcoming, completed, pending);
                await db.SaveChangesAsync();

                var current = new Mock<ICurrentUserService>();
                current.SetupGet(x => x.IsAuthenticated).Returns(true);

                var earnings = new Mock<IEarningsService>();
                var service = new TechnicianDashboardService(db, earnings.Object, current.Object);

                var fixture = new TechnicianFixture(db, service, earnings, current, technician.Id, technicianUser.Id, customer.Id);
                return fixture;
            }
        }
    }
}

