using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;
using Moq;
using Xunit;

namespace ITServicesApp.Tests.Unit.Services
{
    public class EarningsServiceTests
    {
        [Fact]
        public async Task GetSummaryAsync_ComputesNetAtNinetyPercent()
        {
            var options = InMemoryDb.CreateOptions();
            await using var db = new ApplicationDbContext(options);

            var technicianUser = new User { Email = "tech@test", FullName = "Tech", PasswordHash = "hash", Role = UserRole.Technician };
            var customer = new User { Email = "cust@test", FullName = "Customer", PasswordHash = "hash", Role = UserRole.Customer };
            await db.Users.AddRangeAsync(technicianUser, customer);
            await db.SaveChangesAsync();

            var category = new ServiceCategory { Name = "General", Description = "General" };
            await db.ServiceCategories.AddAsync(category);
            await db.SaveChangesAsync();

            var issue = new ServiceIssue { ServiceCategoryId = category.Id, Name = "Install", EstimatedDurationMinutes = 60, BasePrice = 100m };
            await db.ServiceIssues.AddAsync(issue);
            await db.SaveChangesAsync();

            var technician = new Technician { UserId = technicianUser.Id, ServiceCategoryId = category.Id, IsActive = true, User = technicianUser };
            await db.Technicians.AddAsync(technician);
            await db.SaveChangesAsync();

            var booking = new Booking
            {
                UserId = customer.Id,
                TechnicianId = technician.Id,
                Technician = technician,
                User = customer,
                ServiceCategoryId = category.Id,
                ServiceIssueId = issue.Id,
                ServiceIssue = issue,
                ScheduledStartUtc = DateTime.UtcNow.AddDays(-1),
                ScheduledEndUtc = DateTime.UtcNow.AddDays(-1).AddHours(2),
                Status = BookingStatus.Completed
            };
            await db.Bookings.AddAsync(booking);
            await db.SaveChangesAsync();

            await db.Invoices.AddAsync(new Invoice
            {
                BookingId = booking.Id,
                Number = "INV-1",
                IssuedAtUtc = DateTime.UtcNow.AddDays(-1),
                Subtotal = 100m,
                Tax = 10m,
                Total = 110m,
                Currency = "USD"
            });
            await db.SaveChangesAsync();

            var invoicesRepo = new Mock<IInvoiceRepository>();
            var settingsRepo = new Mock<ISettingsRepository>();
            settingsRepo.Setup(s => s.GetSingletonAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlatformSettings { TechnicianCommissionRate = 0.2m, Currency = "USD" });

            var current = new Mock<ICurrentUserService>();
            current.SetupGet(x => x.Role).Returns(UserRole.Admin.ToString());
            current.SetupGet(x => x.IsAuthenticated).Returns(true);

            var service = new EarningsService(invoicesRepo.Object, settingsRepo.Object, db, current.Object);

            var result = await service.GetSummaryAsync(technician.Id, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CancellationToken.None);

            result.Gross.Should().Be(110m);
            result.Net.Should().Be(99m);
            result.CommissionFees.Should().Be(11m);
        }
    }
}
