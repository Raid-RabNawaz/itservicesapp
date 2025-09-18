using System;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Tests.Unit.TestHelpers
{
    public static class TestData
    {
        public static async Task SeedBasicAsync(ApplicationDbContext db)
        {
            var admin = new User { Email = "admin@test", FullName = "Admin", Role = UserRole.Admin, PasswordHash = "x" };
            var techUser = new User { Email = "tech@test", FullName = "Tech", Role = UserRole.Technician, PasswordHash = "x" };
            var customer = new User { Email = "cust@test", FullName = "Customer", Role = UserRole.Customer, PasswordHash = "x" };
            await db.Users.AddRangeAsync(admin, techUser, customer);
            await db.SaveChangesAsync();

            var cat = new ServiceCategory { Name = "Networking", Description = "Net" };
            await db.ServiceCategories.AddAsync(cat);
            await db.SaveChangesAsync();

            var issue = new ServiceIssue { ServiceCategoryId = cat.Id, Name = "Slow WiFi", EstimatedDurationMinutes = 60, BasePrice = 120m };
            await db.ServiceIssues.AddAsync(issue);
            await db.SaveChangesAsync();

            var tech = new Technician { UserId = techUser.Id, ServiceCategoryId = cat.Id, Bio = "Pro", HourlyRate = 50, IsActive = true };
            await db.Technicians.AddAsync(tech);
            await db.SaveChangesAsync();

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            await db.TechnicianSlots.AddAsync(new TechnicianSlot { TechnicianId = tech.Id, StartUtc = tomorrow.AddHours(9), EndUtc = tomorrow.AddHours(17) });
            await db.SaveChangesAsync();
        }

        public static async Task<int> CreateConfirmedBookingAsync(ApplicationDbContext db, DateTime startUtc, string? reminderJobId = null)
        {
            var tech = await db.Technicians.FirstAsync();
            var user = await db.Users.FirstAsync(u => u.Role == UserRole.Customer);
            var cat = await db.ServiceCategories.FirstAsync();
            var iss = await db.ServiceIssues.FirstAsync(i => i.ServiceCategoryId == cat.Id);

            var b = new Booking
            {
                UserId = user.Id,
                TechnicianId = tech.Id,
                ServiceCategoryId = cat.Id,
                ServiceIssueId = iss.Id,
                ScheduledStartUtc = startUtc,
                ScheduledEndUtc = startUtc.AddHours(1),
                Status = BookingStatus.Confirmed,
                ReminderJobId = reminderJobId
            };
            await db.Bookings.AddAsync(b);
            await db.SaveChangesAsync();
            return b.Id;
        }

        public static async Task EnsureSlotForAsync(ApplicationDbContext db, int technicianId, DateTime dayUtc, int startHour = 9, int endHour = 17)
        {
            var dayStart = dayUtc.Date.AddHours(startHour);
            var dayEnd = dayUtc.Date.AddHours(endHour);

            var exists = await db.TechnicianSlots
                .AnyAsync(s => s.TechnicianId == technicianId && s.StartUtc == dayStart && s.EndUtc == dayEnd);

            if (!exists)
            {
                await db.TechnicianSlots.AddAsync(new TechnicianSlot
                {
                    TechnicianId = technicianId,
                    StartUtc = dayStart,
                    EndUtc = dayEnd
                });
                await db.SaveChangesAsync();
            }
        }
    }
}

