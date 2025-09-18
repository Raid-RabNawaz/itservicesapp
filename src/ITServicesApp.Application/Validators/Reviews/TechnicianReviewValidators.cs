using FluentValidation;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.Reviews
{
    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator(
            IBookingRepository bookings,
            ITechnicianRepository techs,
            ITechnicianReviewRepository reviews)
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await bookings.GetByIdAsync(id, ct)) != null)
                .WithMessage("Booking does not exist.");

            RuleFor(x => x.TechnicianId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await techs.GetByIdAsync(id, ct)) != null)
                .WithMessage("Technician does not exist.");

            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Comment).MaximumLength(2000).When(x => x.Comment != null);

            // One review per booking
            RuleFor(x => x.BookingId).MustAsync(async (bookingId, ct) =>
                !(await reviews.ExistsForBookingAsync(bookingId, ct)))
                .WithMessage("A review already exists for this booking.");

            // Ensure the booking is for the same technician and not cancelled
            RuleFor(x => x).MustAsync(async (dto, ct) =>
            {
                var b = await bookings.GetByIdAsync(dto.BookingId, ct);
                if (b == null) return false;
                if (b.Status == BookingStatus.Cancelled) return false;
                return b.TechnicianId == dto.TechnicianId;
            }).WithMessage("Booking and technician do not match or booking is cancelled.");
        }
    }
}
