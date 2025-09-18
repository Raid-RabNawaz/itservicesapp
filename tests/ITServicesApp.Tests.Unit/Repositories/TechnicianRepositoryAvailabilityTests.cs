using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Persistence;
using ITServicesApp.Persistence.Repositories;
using ITServicesApp.Tests.Unit.TestHelpers;
using Xunit;

public class TechnicianRepositoryAvailabilityTests
{
    [Fact]
    public async Task AnyFreeAsync_respects_slots_bookings_and_unavailability()
    {
        var opts = InMemoryDb.CreateOptions();
        await using var db = new ApplicationDbContext(opts);
        await TestData.SeedBasicAsync(db);

        // Create one active technician with a broad slot 9–17
        var cat = db.ServiceCategories.First();
        var techUser = new User { Email = "t@test", FullName = "Tech", Role = UserRole.Technician, PasswordHash = "x" };
        var tech = new Technician { User = techUser, ServiceCategoryId = cat.Id, HourlyRate = 100, IsActive = true };
        await db.AddAsync(tech);
        await db.SaveChangesAsync();

        var day = DateTime.UtcNow.Date.AddDays(1);
        await db.TechnicianSlots.AddAsync(new TechnicianSlot
        {
            TechnicianId = tech.Id,
            StartUtc = day.AddHours(9),
            EndUtc = day.AddHours(17)
        });
        await db.SaveChangesAsync();

        var repo = new TechnicianRepository(db);
        var ids = new[] { tech.Id };
        var ct = CancellationToken.None;

        // Initially free at 11–12
        (await repo.AnyFreeAsync(ids, day.AddHours(11), day.AddHours(12), ct)).Should().BeTrue();

        // Add unavailability 10–12 -> becomes busy
        await db.TechnicianUnavailabilities.AddAsync(new TechnicianUnavailability
        {
            TechnicianId = tech.Id,
            StartUtc = day.AddHours(10),
            EndUtc = day.AddHours(12),
            Reason = "Errand"
        });
        await db.SaveChangesAsync();

        (await repo.AnyFreeAsync(ids, day.AddHours(11), day.AddHours(11.5), ct)).Should().BeFalse();

        // After 12:30 free again
        (await repo.AnyFreeAsync(ids, day.AddHours(12.5), day.AddHours(13.0), ct)).Should().BeTrue();

        // Add a booking overlap 13–14 -> busy
        await db.Bookings.AddAsync(new Booking
        {
            UserId = db.Users.First().Id,
            TechnicianId = tech.Id,
            ServiceCategoryId = cat.Id,
            ServiceIssueId = db.ServiceIssues.First(i => i.ServiceCategoryId == cat.Id).Id,
            ScheduledStartUtc = day.AddHours(13),
            ScheduledEndUtc = day.AddHours(14),
            Status = BookingStatus.Confirmed
        });
        await db.SaveChangesAsync();

        (await repo.AnyFreeAsync(ids, day.AddHours(13.25), day.AddHours(13.75), ct)).Should().BeFalse();
    }
}
