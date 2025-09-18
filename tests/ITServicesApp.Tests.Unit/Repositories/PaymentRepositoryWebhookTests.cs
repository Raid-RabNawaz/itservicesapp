using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Persistence;
using ITServicesApp.Persistence.Repositories;
using ITServicesApp.Tests.Unit.TestHelpers;
using Xunit;

public class PaymentRepositoryWebhookTests
{
    [Fact]
    public async Task WebhookSeen_and_MarkWebhookSeen_work_as_expected()
    {
        var opts = InMemoryDb.CreateOptions();
        await using var db = new ApplicationDbContext(opts);
        await TestData.SeedBasicAsync(db);
        var datetime= new FakeDateTimeProvider();
        // minimal booking + payment
        var booking = await TestData.CreateConfirmedBookingAsync(db, datetime.UtcNow);
        var payment = new Payment { BookingId = booking, Amount = 100, Status = "Pending" };
        await db.Payments.AddAsync(payment);
        await db.SaveChangesAsync();

        var repo = new PaymentRepository(db);
        var ct = CancellationToken.None;
        var eventId = "evt_123";

        (await repo.WebhookSeenAsync(payment.Id, eventId, ct)).Should().BeFalse();

        await repo.MarkWebhookSeenAsync(payment.Id, eventId, ct);

        (await repo.WebhookSeenAsync(payment.Id, eventId, ct)).Should().BeTrue();
        (await repo.WebhookSeenAsync(payment.Id, "evt_other", ct)).Should().BeFalse();
    }
}
