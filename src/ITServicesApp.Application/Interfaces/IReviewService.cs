using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IReviewService
    {
        Task<TechnicianReviewDto> CreateAsync(int userId, CreateReviewDto dto, CancellationToken ct = default);
        Task<IReadOnlyList<TechnicianReviewDto>> ListByTechnicianAsync(int technicianId, int take, int skip, CancellationToken ct = default);
        Task<double> GetAverageAsync(int technicianId, CancellationToken ct = default);
    }
}
