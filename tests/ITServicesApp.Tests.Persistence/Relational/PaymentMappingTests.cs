using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Tests.Persistence.Relational
{
    public class PaymentMappingTests
    {
        [Fact]
        public async Task Payment_is_linked_to_booking_and_updates_status()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);
            await TestData.SeedBasicAsync(db);

            var start = System.DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            var bookingId = await TestData.CreateConfirmedBookingAsync(db, start);

            var p = new Payment
            {
                BookingId = bookingId,
                Method = PaymentMethod.Card,
                Amount = 99.95m,
                Currency = "USD",
                Status = "Initiated"
            };
            db.Payments.Add(p);
            await db.SaveChangesAsync();

            var loaded = await db.Payments.Include(x => x.Booking).FirstAsync();
            loaded.BookingId.Should().Be(bookingId);
            loaded.Status = "Succeeded";
            await db.SaveChangesAsync();

            var updated = await db.Payments.FirstAsync();
            updated.Status.Should().Be("Succeeded");
        }
    }
}
