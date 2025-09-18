using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ServiceCatalogService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ServiceCategoryDto>> ListCategoriesAsync(CancellationToken ct = default)
        {
            var list = await _uow.ServiceCategories.ListAllAsync(ct);
            return list.Select(_mapper.Map<ServiceCategoryDto>).ToList();
        }

        public async Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<ServiceCategory>(dto);
            await _uow.ServiceCategories.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<ServiceCategoryDto>(entity);
        }

        public async Task UpdateCategoryAsync(int id, UpdateServiceCategoryDto dto, CancellationToken ct = default)
        {
            var c = await _uow.ServiceCategories.GetByIdAsync(id, ct) ?? throw new System.InvalidOperationException("Category not found.");
            _mapper.Map(dto, c);
            _uow.ServiceCategories.Update(c);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeleteCategoryAsync(int id, CancellationToken ct = default)
        {
            if (await _uow.ServiceCategories.HasIssuesAsync(id, ct))
                throw new System.InvalidOperationException("Category has issues and cannot be deleted.");
            var c = await _uow.ServiceCategories.GetByIdAsync(id, ct) ?? throw new System.InvalidOperationException("Category not found.");
            _uow.ServiceCategories.Delete(c);
            await _uow.SaveChangesAsync(ct);
        }

        public Task<IReadOnlyList<ServiceIssueDto>> ListIssuesAsync(int categoryId, CancellationToken ct = default)
            => MapList(_uow.ServiceIssues.ListByCategoryAsync(categoryId, ct));

        public async Task<ServiceIssueDto> CreateIssueAsync(CreateServiceIssueDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<ServiceIssue>(dto);
            await _uow.ServiceIssues.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<ServiceIssueDto>(entity);
        }

        public async Task UpdateIssueAsync(int id, UpdateServiceIssueDto dto, CancellationToken ct = default)
        {
            var i = await _uow.ServiceIssues.GetByIdAsync(id, ct) ?? throw new System.InvalidOperationException("Issue not found.");
            _mapper.Map(dto, i);
            _uow.ServiceIssues.Update(i);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeleteIssueAsync(int id, CancellationToken ct = default)
        {
            var i = await _uow.ServiceIssues.GetByIdAsync(id, ct) ?? throw new System.InvalidOperationException("Issue not found.");
            _uow.ServiceIssues.Delete(i);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<IReadOnlyList<ServiceIssueDto>> MapList(Task<IReadOnlyList<ServiceIssue>> t)
        {
            var list = await t;
            return list.Select(_mapper.Map<ServiceIssueDto>).ToList();
        }

        public async Task<IReadOnlyList<ServiceIssueDto>> ListIssuesByCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            var issues = await _uow.ServiceIssues.ListByCategoryAsync(categoryId, ct);
            return issues.Select(_mapper.Map<ServiceIssueDto>).ToList();
        }

    }
}
