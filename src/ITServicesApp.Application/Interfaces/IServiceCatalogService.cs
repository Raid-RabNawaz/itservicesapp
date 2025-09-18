using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IServiceCatalogService
    {
        Task<IReadOnlyList<ServiceCategoryDto>> ListCategoriesAsync(CancellationToken ct = default);
        Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryDto dto, CancellationToken ct = default);
        Task UpdateCategoryAsync(int id, UpdateServiceCategoryDto dto, CancellationToken ct = default);
        Task DeleteCategoryAsync(int id, CancellationToken ct = default); // checks Restrict rules

        Task<IReadOnlyList<ServiceIssueDto>> ListIssuesAsync(int categoryId, CancellationToken ct = default);
        Task<ServiceIssueDto> CreateIssueAsync(CreateServiceIssueDto dto, CancellationToken ct = default);
        Task UpdateIssueAsync(int id, UpdateServiceIssueDto dto, CancellationToken ct = default);
        Task DeleteIssueAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<ServiceIssueDto>> ListIssuesByCategoryAsync(int categoryId, CancellationToken ct = default);

    }
}
