using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo; private readonly ICurrentUserService _me; private readonly ApplicationDbContext _db;
        public AddressService(IAddressRepository repo, ICurrentUserService me, ApplicationDbContext db) { _repo = repo; _me = me; _db = db; }
        public async Task<IReadOnlyList<AddressDto>> ListMineAsync(int take, int skip, CancellationToken ct)
        {
            var list = await _repo.ListByUserAsync(_me.UserIdInt, take, skip, ct);
            return list.Select(x => new AddressDto { Id = x.Id, UserId = x.UserId, Label = x.Label, Line1 = x.Line1, Line2 = x.Line2, City = x.City, State = x.State, PostalCode = x.PostalCode, Country = x.Country, IsDefault = x.IsDefault, CreatedAtUtc = x.CreatedAtUtc }).ToList();
        }
        public async Task<AddressDto> CreateAsync(CreateAddressDto dto, CancellationToken ct)
        {
            var entity = new Address { UserId = _me.UserIdInt, Label = dto.Label, Line1 = dto.Line1, Line2 = dto.Line2, City = dto.City, State = dto.State, PostalCode = dto.PostalCode, Country = dto.Country, IsDefault = dto.IsDefault, CreatedAtUtc = DateTime.UtcNow };
            await _repo.AddAsync(entity, ct); await _db.SaveChangesAsync(ct);
            if (entity.IsDefault)
            {
                var others = await _db.Addresses.Where(a => a.UserId == _me.UserIdInt && a.Id != entity.Id && a.IsDefault).ToListAsync(ct);
                foreach (var o in others) o.IsDefault = false;
                await _db.SaveChangesAsync(ct);
            }
            return new AddressDto { Id = entity.Id, UserId = entity.UserId, Label = entity.Label, Line1 = entity.Line1, Line2 = entity.Line2, City = entity.City, State = entity.State, PostalCode = entity.PostalCode, Country = entity.Country, IsDefault = entity.IsDefault, CreatedAtUtc = entity.CreatedAtUtc };
        }
        public async Task UpdateAsync(int id, UpdateAddressDto dto, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
            if (entity.UserId != _me.UserIdInt) throw new UnauthorizedAccessException();
            entity.Label = dto.Label; entity.Line1 = dto.Line1; entity.Line2 = dto.Line2; entity.City = dto.City; entity.State = dto.State; entity.PostalCode = dto.PostalCode; entity.Country = dto.Country; entity.IsDefault = dto.IsDefault;
            _repo.Update(entity); await _db.SaveChangesAsync(ct);
        }
        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
            if (entity.UserId != _me.UserIdInt) throw new UnauthorizedAccessException();
            _repo.Delete(entity); await _db.SaveChangesAsync(ct);
        }
        public async Task SetDefaultAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
            if (entity.UserId != _me.UserIdInt) throw new UnauthorizedAccessException();
            entity.IsDefault = true; _repo.Update(entity); await _db.SaveChangesAsync(ct);
            var others = await _db.Addresses.Where(a => a.UserId == _me.UserIdInt && a.Id != entity.Id && a.IsDefault).ToListAsync(ct);
            foreach (var o in others) o.IsDefault = false;
            await _db.SaveChangesAsync(ct);
        }
    }
}