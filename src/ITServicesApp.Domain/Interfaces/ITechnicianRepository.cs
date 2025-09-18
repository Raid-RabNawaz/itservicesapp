using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface ITechnicianRepository : IRepository<Technician>
    {
        Task<List<int>> QueryQualifiedTechnicianIdsAsync(int serviceCategoryId, int serviceIssueId, CancellationToken ct);
        Task<bool> AnyFreeAsync(IEnumerable<int> technicianIds, DateTime start, DateTime end, CancellationToken ct);
        Task<Technician?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
