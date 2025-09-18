using System;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;
using Xunit;

namespace ITServicesApp.Tests.Unit.Services
{
    public class BookingAssignmentServiceTests
    {
        [Fact]
        public async Task IsTechnicianAvailableAsync_AllowsSlotThatSpansMidnight()
        {
            var options = InMemoryDb.CreateOptions();
            await using var db = new ApplicationDbContext(options);
            var (tech, _) = await SeedTechnicianAsync(db,
                new DateTime(2025, 10, 10, 22, 0, 0, DateTimeKind.Utc),
                new DateTime(2025, 10, 11, 2, 0, 0, DateTimeKind.Utc));

            var uow = TestUowFactory.Create(db);
            var service = new BookingAssignmentService(uow);

            var start = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
            var available = await service.IsTechnicianAvailableAsync(tech.Id, start, 60);

            available.Should().BeTrue();
        }

        [Fact]
        public async Task IsTechnicianAvailableAsync_False_WhenUnavailabilityOverlapsAcrossMidnight()
        {
            var options = InMemoryDb.CreateOptions();
            await using var db = new ApplicationDbContext(options);
            var (tech, _) = await SeedTechnicianAsync(db,
                new DateTime(2025, 10, 10, 22, 0, 0, DateTimeKind.Utc),
                new DateTime(2025, 10, 11, 2, 0, 0, DateTimeKind.Utc),
                new DateTime(2025, 10, 11, 0, 30, 0, DateTimeKind.Utc),
                new DateTime(2025, 10, 11, 1, 30, 0, DateTimeKind.Utc));

            var uow = TestUowFactory.Create(db);
            var service = new BookingAssignmentService(uow);

            var start = new DateTime(2025, 10, 10, 23, 30, 0, DateTimeKind.Utc);
            var available = await service.IsTechnicianAvailableAsync(tech.Id, start, 120);

            available.Should().BeFalse();
        }

        [Fact]
        public async Task FindBestAsync_ReturnsTechnicianForCrossMidnightSlot()
        {
            var options = InMemoryDb.CreateOptions();
            await using var db = new ApplicationDbContext(options);
            var (tech, issue) = await SeedTechnicianAsync(db,
                new DateTime(2025, 10, 10, 22, 0, 0, DateTimeKind.Utc),
                new DateTime(2025, 10, 11, 2, 0, 0, DateTimeKind.Utc));

            var uow = TestUowFactory.Create(db);
            var service = new BookingAssignmentService(uow);

            var requestStart = new DateTime(2025, 10, 10, 23, 0, 0, DateTimeKind.Utc);
            var result = await service.FindBestAsync(tech.ServiceCategoryId, issue.Id, requestStart, 90);

            result.Should().NotBeNull();
            result!.TechnicianId.Should().Be(tech.Id);
            result.SlotId.Should().NotBeNull();
            result.StartUtc.Should().Be(requestStart);
            result.EndUtc.Should().Be(requestStart.AddMinutes(90));
        }

        private static async Task<(Technician Tech, ServiceIssue Issue)> SeedTechnicianAsync(
            ApplicationDbContext db,
            DateTime slotStart,
            DateTime slotEnd,
            DateTime? unavailStart = null,
            DateTime? unavailEnd = null)
        {
            var techUser = new User
            {
                Email = "midnight-tech@test",
                FullName = "Midnight Tech",
                Role = UserRole.Technician,
                PasswordHash = "hash"
            };
            await db.Users.AddAsync(techUser);
            await db.SaveChangesAsync();

            var category = new ServiceCategory
            {
                Name = "After Hours",
                Description = "Late shifts"
            };
            await db.ServiceCategories.AddAsync(category);
            await db.SaveChangesAsync();

            var issue = new ServiceIssue
            {
                ServiceCategoryId = category.Id,
                Name = "Night install",
                EstimatedDurationMinutes = 90,
                BasePrice = 150m
            };
            await db.ServiceIssues.AddAsync(issue);
            await db.SaveChangesAsync();

            var tech = new Technician
            {
                UserId = techUser.Id,
                ServiceCategoryId = category.Id,
                IsActive = true,
                HourlyRate = 120m
            };
            await db.Technicians.AddAsync(tech);
            await db.SaveChangesAsync();

            await db.TechnicianSlots.AddAsync(new TechnicianSlot
            {
                TechnicianId = tech.Id,
                StartUtc = slotStart,
                EndUtc = slotEnd
            });
            await db.SaveChangesAsync();

            if (unavailStart.HasValue && unavailEnd.HasValue)
            {
                await db.TechnicianUnavailabilities.AddAsync(new TechnicianUnavailability
                {
                    TechnicianId = tech.Id,
                    StartUtc = unavailStart.Value,
                    EndUtc = unavailEnd.Value,
                    Reason = "Overlapping event"
                });
                await db.SaveChangesAsync();
            }

            return (tech, issue);
        }
    }
}
