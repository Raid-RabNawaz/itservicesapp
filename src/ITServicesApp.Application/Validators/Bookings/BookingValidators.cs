using System;
using System.Linq;
using FluentValidation;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.Bookings
{
    public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDtoValidator(
            IUserRepository users,
            ITechnicianRepository techs,
            IServiceCategoryRepository categories,
            IServiceIssueRepository issues,
            ITechnicianSlotRepository slots,
            IBookingRepository bookings,
            IDateTimeProvider clock)
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await users.GetByIdAsync(id, ct)) != null)
                .WithMessage("User does not exist.");

            RuleFor(x => x.TechnicianId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await techs.GetByIdAsync(id, ct)) != null)
                .WithMessage("Technician does not exist.");

            When(x => x.ServiceCategoryId.HasValue && x.ServiceCategoryId.Value > 0, () =>
            {
                RuleFor(x => x.ServiceCategoryId!.Value)
                    .MustAsync(async (id, ct) => (await categories.GetByIdAsync(id, ct)) != null)
                    .WithMessage("Service category does not exist.");
            });

            When(x => x.ServiceIssueId.HasValue && x.ServiceIssueId.Value > 0, () =>
            {
                RuleFor(x => x.ServiceIssueId!.Value)
                    .MustAsync(async (id, ct) => (await issues.GetByIdAsync(id, ct)) != null)
                    .WithMessage("Service issue does not exist.");
            });

            RuleFor(x => x)
                .Must(x => (x.ServiceIssueId.HasValue && x.ServiceIssueId.Value > 0) || (x.Items != null && x.Items.Count > 0))
                .WithMessage("At least one service must be selected.");

            RuleForEach(x => x.Items)
                .SetValidator(new CreateBookingItemDtoValidator(issues));

            RuleFor(x => x.Start).LessThan(x => x.End).WithMessage("Start must be before End.");

            RuleFor(x => x.Start)
                .Must(start => start > clock.UtcNow.AddMinutes(-1))
                .WithMessage("Start time must be in the future.");

            RuleFor(x => x).MustAsync(async (dto, ct) =>
            {
                var daySlots = await slots.GetAvailableAsync(dto.TechnicianId, dto.Start.Date, ct);
                return daySlots.Any(s => s.StartUtc <= dto.Start && dto.End <= s.EndUtc);
            }).WithMessage("Selected technician has no available slot covering the requested time.");

            RuleFor(x => x).MustAsync(async (dto, ct) =>
                !(await bookings.HasOverlapAsync(dto.TechnicianId, dto.Start, dto.End, ct)))
                .WithMessage("The technician is already booked in the selected time range.");
        }
    }

    public class UpdateBookingNotesDtoValidator : AbstractValidator<UpdateBookingNotesDto>
    {
        public UpdateBookingNotesDtoValidator(IBookingRepository bookings)
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await bookings.GetByIdAsync(id, ct)) != null)
                .WithMessage("Booking does not exist.");

            RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes != null);
        }
    }

    public class CancelBookingDtoValidator : AbstractValidator<CancelBookingDto>
    {
        public CancelBookingDtoValidator(IBookingRepository bookings, IDateTimeProvider clock)
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await bookings.GetByIdAsync(id, ct)) != null)
                .WithMessage("Booking does not exist.");

            RuleFor(x => x.BookingId).MustAsync(async (id, ct) =>
            {
                var b = await bookings.GetByIdAsync(id, ct);
                if (b == null) return false;
                if (b.Status == BookingStatus.Cancelled) return false;
                return b.ScheduledStartUtc - clock.UtcNow >= TimeSpan.FromHours(24);
            }).WithMessage("Cannot cancel within 24 hours of the visit or booking already cancelled.");
        }
    }

    public class CreateBookingItemDtoValidator : AbstractValidator<CreateBookingItemDto>
    {
        public CreateBookingItemDtoValidator(IServiceIssueRepository issues)
        {
            RuleFor(x => x.ServiceIssueId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await issues.GetByIdAsync(id, ct)) != null)
                .WithMessage("Service issue does not exist.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be at least 1.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);

        }
    }
}


