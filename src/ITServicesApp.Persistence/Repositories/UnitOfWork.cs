using System.Data;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _db;
        private IDbContextTransaction? _tx;

        public UnitOfWork(
            ApplicationDbContext db,
            IBookingRepository bookings,
            IBookingItemRepository bookingItems,
            IBookingDraftRepository bookingDrafts,
            ITechnicianSlotRepository slots,
            IUserRepository users,
            ITechnicianRepository technicians,
            ITechnicianExpertiseRepository technicianExpertises,
            IServiceCategoryRepository serviceCategories,
            IServiceIssueRepository serviceIssues,
            INotificationRepository notifications,
            IPaymentRepository payments,
            ITechnicianUnavailabilityRepository technicianUnavailabilities,
            ITechnicianReviewRepository technicianReviews)
        {
            _db = db;
            Bookings = bookings;
            BookingItems = bookingItems;
            BookingDrafts = bookingDrafts;
            TechnicianSlots = slots;
            Users = users;
            Technicians = technicians;
            TechnicianExpertises = technicianExpertises;
            ServiceCategories = serviceCategories;
            ServiceIssues = serviceIssues;
            Notifications = notifications;
            TechnicianUnavailabilities = technicianUnavailabilities;
            TechnicianReviews = technicianReviews;
            Payments = payments;
        }

        public IBookingRepository Bookings { get; }
        public IBookingItemRepository BookingItems { get; }
        public IBookingDraftRepository BookingDrafts { get; }
        public ITechnicianSlotRepository TechnicianSlots { get; }
        public IUserRepository Users { get; }
        public ITechnicianRepository Technicians { get; }
        public ITechnicianExpertiseRepository TechnicianExpertises { get; }
        public IServiceCategoryRepository ServiceCategories { get; }
        public IServiceIssueRepository ServiceIssues { get; }
        public INotificationRepository Notifications { get; }
        public ITechnicianUnavailabilityRepository TechnicianUnavailabilities { get; }
        public ITechnicianReviewRepository TechnicianReviews { get; }
        public IPaymentRepository Payments { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_tx != null) return _tx;
            _tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            return _tx;
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_tx == null) return;
            await _tx.CommitAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_tx == null) return;
            await _tx.RollbackAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public void Dispose()
        {
            _tx?.Dispose();
        }
    }
}
