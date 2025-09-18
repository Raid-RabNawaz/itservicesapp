using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IBookingRepository Bookings { get; }
        IBookingItemRepository BookingItems { get; }
        IBookingDraftRepository BookingDrafts { get; }
        ITechnicianSlotRepository TechnicianSlots { get; }
        IUserRepository Users { get; }
        ITechnicianRepository Technicians { get; }
        ITechnicianExpertiseRepository TechnicianExpertises { get; }
        IServiceCategoryRepository ServiceCategories { get; }
        IServiceIssueRepository ServiceIssues { get; }
        INotificationRepository Notifications { get; }
        ITechnicianUnavailabilityRepository TechnicianUnavailabilities { get; }
        ITechnicianReviewRepository TechnicianReviews { get; }
        IPaymentRepository Payments { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
