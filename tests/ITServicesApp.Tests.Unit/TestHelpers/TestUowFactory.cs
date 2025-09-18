using ITServicesApp.Persistence;
using ITServicesApp.Persistence.Repositories;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Tests.Unit.TestHelpers
{
    public static class TestUowFactory
    {
        public static IUnitOfWork Create(ApplicationDbContext db)
        {
            var bookings = new BookingRepository(db);
            var bookingItems = new BookingItemRepository(db);
            var drafts = new BookingDraftRepository(db);
            var slots = new TechnicianSlotRepository(db);
            var users = new UserRepository(db);
            var techs = new TechnicianRepository(db);
            var techExpertise = new TechnicianExpertiseRepository(db);
            var cats = new ServiceCategoryRepository(db);
            var issues = new ServiceIssueRepository(db);
            var notes = new NotificationRepository(db);
            var payments = new PaymentRepository(db);
            var unavails = new TechnicianUnavailabilityRepository(db);
            var reviews = new TechnicianReviewRepository(db);

            return new UnitOfWork(db, bookings, bookingItems, drafts, slots, users, techs, techExpertise, cats, issues, notes, payments, unavails, reviews);
        }
    }
}
