using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ITServicesApp.Tests.Persistence.Relational;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Persistence;

public class CascadeRulesTests
{
    [Fact]
    public async Task Deleting_user_is_restricted_when_bookings_exist()
    {
        var (conn, opts) = SqliteRelationalDb.CreateOpen();
        try
        {
            await using var db = new ApplicationDbContext(opts);
            SqliteRelationalDb.EnsureCreatedWithFks(db);

            // Build a minimal valid graph
            var user = new User { Email = "cust@test", FullName = "Cust", Role = UserRole.Customer, PasswordHash = "x" };
            var cat = new ServiceCategory { Name = "General", Description = "General" };
            var iss = new ServiceIssue { Name = "Install", Description = "Install", EstimatedDurationMinutes = 60, ServiceCategory = cat };

            var techUser = new User { Email = "tech@test", FullName = "Tech", Role = UserRole.Technician, PasswordHash = "x" };
            var tech = new Technician { User = techUser, ServiceCategory = cat, HourlyRate = 100m, IsActive = true };

            await db.AddRangeAsync(user, cat, iss, techUser, tech);
            await db.SaveChangesAsync();

            var booking = new Booking
            {
                UserId = user.Id,
                TechnicianId = tech.Id,
                ServiceCategoryId = cat.Id,
                ServiceIssueId = iss.Id,
                ScheduledStartUtc = DateTime.UtcNow.AddDays(1),
                ScheduledEndUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
                Status = BookingStatus.Confirmed
            };
            await db.Bookings.AddAsync(booking);
            await db.SaveChangesAsync();

            // Attempt to delete the principal
            db.Users.Remove(user);

            // Expect relational FK to block this on SaveChanges
            Func<Task> act = async () => await db.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();

            // And the user should still exist
            var stillExists = await db.Users.AnyAsync(u => u.Id == user.Id);
            stillExists.Should().BeTrue();
        }
        finally
        {
            await conn.DisposeAsync();
        }
    }

    [Fact]
    public async Task Deleting_technician_is_restricted_when_bookings_exist()
    {
        var (conn, opts) = SqliteRelationalDb.CreateOpen();
        try
        {
            await using var db = new ApplicationDbContext(opts);
            SqliteRelationalDb.EnsureCreatedWithFks(db);

            var cat = new ServiceCategory { Name = "General" };
            var issue = new ServiceIssue { Name = "Install", ServiceCategory = cat, EstimatedDurationMinutes = 60 };
            var uCust = new User { Email = "cust@x", FullName = "C", PasswordHash = "h", Role = UserRole.Customer };
            var uTech = new User { Email = "tech@x", FullName = "T", PasswordHash = "h", Role = UserRole.Technician };
            var tech = new Technician { User = uTech, ServiceCategory = cat, IsActive = true, HourlyRate = 100 };

            await db.AddRangeAsync(cat, issue, uCust, tech);
            await db.SaveChangesAsync();

            var when = DateTime.UtcNow.AddDays(1);
            await db.Bookings.AddAsync(new Booking
            {
                UserId = uCust.Id,
                TechnicianId = tech.Id,
                ServiceCategoryId = cat.Id,
                ServiceIssueId = issue.Id,
                ScheduledStartUtc = when,
                ScheduledEndUtc = when.AddHours(1),
                Status = BookingStatus.Confirmed
            });
            await db.SaveChangesAsync();

            db.Technicians.Remove(tech);

            await FluentActions.Awaiting(() => db.SaveChangesAsync())
                .Should().ThrowAsync<DbUpdateException>();

            (await db.Technicians.AnyAsync(t => t.Id == tech.Id)).Should().BeTrue();
        }
        finally { await conn.DisposeAsync(); }
    }
}
