using System.Security.Cryptography;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITServicesApp.Persistence.Seeds
{
    public static class ApplicationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher hasher, ILogger logger, CancellationToken ct = default)
        {
            await db.Database.EnsureCreatedAsync(ct);

            // USERS
            if (!await db.Users.AnyAsync(ct))
            {
                logger.LogInformation("Seeding users...");

                // Dev passwords (force change on first login)
                var adminPass = "Admin@123!";
                var techPass = "Tech@123!";
                var custPass = "Customer@123!";

                var admin = new User
                {
                    Email = "admin@itservices.local",
                    FullName = "Admin",
                    Role = UserRole.Admin,
                    PasswordHash = hasher.Hash(adminPass),
                    MustChangePassword = true
                };
                var techUser = new User
                {
                    Email = "tech@itservices.local",
                    FullName = "Alex Tech",
                    Role = UserRole.Technician,
                    PasswordHash = hasher.Hash(techPass),
                    MustChangePassword = true
                };
                var customer = new User
                {
                    Email = "customer@itservices.local",
                    FullName = "Casey Customer",
                    Role = UserRole.Customer,
                    PasswordHash = hasher.Hash(custPass),
                    MustChangePassword = true
                };

                await db.Users.AddRangeAsync(new[] { admin, techUser, customer }, ct);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seeded users with first-login passwords (change on first login).");

            }

            // SERVICE CATALOG
            if (!await db.ServiceCategories.AnyAsync(ct))
            {
                logger.LogInformation("Seeding service categories/issues...");

                var networking = new ServiceCategory { Name = "Networking", Description = "WiFi, routers, LAN/WAN" };
                var hardware = new ServiceCategory { Name = "Hardware", Description = "PC builds, diagnostics" };

                await db.ServiceCategories.AddRangeAsync(new[] { networking, hardware }, ct);
                await db.SaveChangesAsync(ct);

                var issues = new[]
                {
                    new ServiceIssue { ServiceCategoryId = networking.Id, Name = "Slow WiFi", Description = "Low throughput", EstimatedDurationMinutes = 60 },
                    new ServiceIssue { ServiceCategoryId = networking.Id, Name = "No Internet", Description = "WAN down", EstimatedDurationMinutes = 90 },
                    new ServiceIssue { ServiceCategoryId = hardware.Id,   Name = "PC Won't Boot", Description = "POST failure", EstimatedDurationMinutes = 120 },
                };
                await db.ServiceIssues.AddRangeAsync(issues, ct);
                await db.SaveChangesAsync(ct);
            }

            // TECHNICIAN + SLOTS
            if (!await db.Technicians.AnyAsync(ct))
            {
                logger.LogInformation("Seeding technician and slots...");

                var techUser = await db.Users.FirstAsync(u => u.Role == UserRole.Technician, ct);
                var networkingCat = await db.ServiceCategories.FirstAsync(c => c.Name == "Networking", ct);

                var tech = new Technician
                {
                    UserId = techUser.Id,
                    ServiceCategoryId = networkingCat.Id,
                    Bio = "Network specialist with 8+ years experience",
                    HourlyRate = 40m,
                    IsActive = true
                };
                await db.Technicians.AddAsync(tech, ct);
                await db.SaveChangesAsync(ct);

                // Slots: next 3 days 09:00-17:00 UTC
                var startDay = DateTime.UtcNow.Date.AddDays(1);
                for (int i = 0; i < 3; i++)
                {
                    var s = startDay.AddDays(i);
                    await db.TechnicianSlots.AddAsync(new TechnicianSlot
                    {
                        TechnicianId = tech.Id,
                        StartUtc = s.AddHours(9),
                        EndUtc = s.AddHours(17)
                    }, ct);
                }
                await db.SaveChangesAsync(ct);
            }

            // SAMPLE BOOKING + PAYMENT
            if (!await db.Bookings.AnyAsync(ct))
            {
                logger.LogInformation("Seeding sample booking/payment...");

                var customer = await db.Users.FirstAsync(u => u.Role == UserRole.Customer, ct);
                var tech = await db.Technicians.Include(t => t.User).FirstAsync(ct);
                var cat = await db.ServiceCategories.FirstAsync(c => c.Name == "Networking", ct);
                var issue = await db.ServiceIssues.FirstAsync(i => i.ServiceCategoryId == cat.Id, ct);

                var tomorrow = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

                var booking = new Booking
                {
                    UserId = customer.Id,
                    TechnicianId = tech.Id,
                    ServiceCategoryId = cat.Id,
                    ServiceIssueId = issue.Id,
                    ScheduledStartUtc = tomorrow,
                    ScheduledEndUtc = tomorrow.AddHours(1.5),
                    Status = BookingStatus.Confirmed,
                    Address = "123 Demo Street",
                    Notes = "Office WiFi keeps dropping",
                    ClientRequestId = "seed-initial"
                };

                await db.Bookings.AddAsync(booking, ct);
                await db.SaveChangesAsync(ct);

                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Method = PaymentMethod.Card,
                    Amount = 59.99m,
                    Currency = "USD",
                    Status = "Pending"
                };
                await db.Payments.AddAsync(payment, ct);
                await db.SaveChangesAsync(ct);
            }

            logger.LogInformation("Seeding complete.");
        }
    }
}
