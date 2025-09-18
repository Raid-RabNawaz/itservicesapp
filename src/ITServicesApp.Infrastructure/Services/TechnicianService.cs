using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public class TechnicianService : ITechnicianService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _current;

        public TechnicianService(IUnitOfWork uow, IMapper mapper, ICurrentUserService current)
        {
            _uow = uow;
            _mapper = mapper;
            _current = current;
        }

        public async Task<TechnicianDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var t = await _uow.Technicians.GetByIdAsync(id, ct);
            return t is null ? null : _mapper.Map<TechnicianDto>(t);
        }

        public async Task<TechnicianProfileDto?> GetProfileByIdAsync(int id, CancellationToken ct = default)
        {
            var t = await _uow.Technicians.GetByIdAsync(id, ct);
            if (t is null) return null;

            var dto = _mapper.Map<TechnicianProfileDto>(t);
            dto.UserFullName = t.User.FullName;
            dto.UserEmail = t.User.Email;
            dto.ServiceCategoryName = (await _uow.ServiceCategories.GetByIdAsync(t.ServiceCategoryId, ct))?.Name ?? "N/A";

            dto.AverageRating = await _uow.TechnicianReviews.GetAverageRatingAsync(id, ct);
            var reviews = await _uow.TechnicianReviews.ListByTechnicianAsync(id, 1, 0, ct);
            dto.ReviewsCount = reviews.Count; // better to query count separately; simplified

            return dto;
        }

        public async Task UpdateProfileAsync(int id, UpdateTechnicianProfileDto dto, CancellationToken ct = default)
        {
            var tech = await _uow.Technicians.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Technician not found.");
            _mapper.Map(dto, tech);
            _uow.Technicians.Update(tech);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<List<TechnicianSlotDto>> GetCalendarAsync(int id, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        {
            var slots = await _uow.TechnicianSlots.ListAsync(s => s.TechnicianId == id && s.StartUtc < toUtc && fromUtc < s.EndUtc, ct);
            var unav = await _uow.TechnicianUnavailabilities.ListForTechnicianAsync(id, fromUtc, toUtc, ct);

            var list = new List<TechnicianSlotDto>();
            list.AddRange(slots.Select(_mapper.Map<TechnicianSlotDto>));
            // Represent unavailability as slots with IsAvailable=false
            list.AddRange(unav.Select(u => new TechnicianSlotDto
            {
                TechnicianId = id,
                StartUtc = u.StartUtc,
                EndUtc = u.EndUtc,
                IsAvailable = false
            }));
            return list.OrderBy(x => x.StartUtc).ToList();
        }

        public async Task<TechnicianUnavailabilityDto> MarkUnavailabilityAsync(CreateUnavailabilityDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<TechnicianUnavailability>(dto);
            await _uow.TechnicianUnavailabilities.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<TechnicianUnavailabilityDto>(entity);
        }

        public async Task DeleteUnavailabilityAsync(int unavailabilityId, CancellationToken ct = default)
        {
            var u = await _uow.TechnicianUnavailabilities.GetByIdAsync(unavailabilityId, ct)
                ?? throw new InvalidOperationException("Unavailability not found.");
            _uow.TechnicianUnavailabilities.Delete(u);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<TechnicianProfileDto> GetMyProfileAsync(CancellationToken ct = default)
        {
            var tech = await _uow.Technicians.ListAsync(t => t.UserId == _current.UserIdInt, ct);
            var entity = tech.FirstOrDefault() ?? throw new InvalidOperationException("Technician profile not found for current user.");
            return _mapper.Map<TechnicianProfileDto>(entity);
        }

        public async Task UpdateMyProfileAsync(UpdateTechnicianProfileDto dto, CancellationToken ct = default)
        {
            var tech = (await _uow.Technicians.ListAsync(t => t.UserId == _current.UserIdInt, ct)).FirstOrDefault()
                       ?? throw new InvalidOperationException("Technician profile not found.");

            // Explicit conditional updates (works whether dto fields are nullable or not)
            if (dto.Bio != null) tech.Bio = dto.Bio;

            if (dto.HourlyRate.HasValue)
                tech.HourlyRate = dto.HourlyRate.Value;

            // If your DTO has non-nullable bool (e.g., default false), add an additional flag or use a separate endpoint.
            // Assuming it's nullable here; handle both cases:
            if (NullableBoolHasValue(dto.IsActive, out var isActive))
                tech.IsActive = isActive;

            _uow.Technicians.Update(tech);
            await _uow.SaveChangesAsync(ct);
        }

        private static bool NullableBoolHasValue(bool? value, out bool result)
        {
            if (value.HasValue) { result = value.Value; return true; }
            result = default; return false;
        }
    }
}
