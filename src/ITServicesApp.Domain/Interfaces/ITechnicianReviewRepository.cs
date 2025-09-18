using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface ITechnicianReviewRepository : IRepository<TechnicianReview>
    {
        Task<bool> ExistsForBookingAsync(int bookingId, CancellationToken ct = default);
        Task<IReadOnlyList<TechnicianReview>> ListByTechnicianAsync(int technicianId, int take, int skip, CancellationToken ct = default);
        Task<double> GetAverageRatingAsync(int technicianId, CancellationToken ct = default);

    }
}
