using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<TechnicianReviewDto> CreateAsync(int userId, CreateReviewDto dto, CancellationToken ct = default)
        {
            var booking = await _uow.Bookings.GetByIdAsync(dto.BookingId, ct) ?? throw new System.InvalidOperationException("Booking not found.");
            if (booking.UserId != userId) throw new System.InvalidOperationException("You can only review your own booking.");
            if (booking.Status == BookingStatus.Cancelled) throw new System.InvalidOperationException("Cannot review a cancelled booking.");
            if (booking.Status != BookingStatus.Completed) throw new System.InvalidOperationException("You can only review completed bookings.");
            if (booking.TechnicianId != dto.TechnicianId) throw new System.InvalidOperationException("Technician mismatch.");
            if (await _uow.TechnicianReviews.ExistsForBookingAsync(dto.BookingId, ct))
                throw new System.InvalidOperationException("Review already exists for this booking.");

            var entity = _mapper.Map<TechnicianReview>(dto);
            entity.UserId = userId;
            await _uow.TechnicianReviews.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<TechnicianReviewDto>(entity);
        }

        public async Task<IReadOnlyList<TechnicianReviewDto>> ListByTechnicianAsync(int technicianId, int take, int skip, CancellationToken ct = default)
        {
            var list = await _uow.TechnicianReviews.ListByTechnicianAsync(technicianId, take, skip, ct);
            return list.Select(_mapper.Map<TechnicianReviewDto>).ToList();
        }

        public Task<double> GetAverageAsync(int technicianId, CancellationToken ct = default)
            => _uow.TechnicianReviews.GetAverageRatingAsync(technicianId, ct);
    }
}
