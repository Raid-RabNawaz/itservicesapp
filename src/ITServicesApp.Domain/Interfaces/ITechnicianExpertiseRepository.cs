using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface ITechnicianExpertiseRepository
    {
        Task AddAsync(TechnicianExpertise expertise, CancellationToken ct = default);
        Task RemoveAsync(int technicianId, int serviceIssueId, CancellationToken ct = default);
        Task<bool> ExistsAsync(int technicianId, int serviceIssueId, CancellationToken ct = default);
        Task<IReadOnlyList<TechnicianExpertise>> ListByTechnicianAsync(int technicianId, CancellationToken ct = default);
    }
}
