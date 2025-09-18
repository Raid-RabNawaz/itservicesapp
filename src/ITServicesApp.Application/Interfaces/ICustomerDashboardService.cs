using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface ICustomerDashboardService
    {
        Task<CustomerDashboardDto> GetAsync(int customerId, CancellationToken ct = default);
    }
}
