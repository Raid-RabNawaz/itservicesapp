using System;
using System.Linq;
using FluentValidation;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.Bookings
{
    public class GuestBookingRequestDtoValidator : AbstractValidator<GuestBookingRequestDto>
    {
        public GuestBookingRequestDtoValidator(
            ITechnicianRepository technicians,
            IServiceCategoryRepository categories,
            IServiceIssueRepository issues,
            ITechnicianSlotRepository slots,
            IBookingRepository bookings,
            IDateTimeProvider clock)
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.TechnicianId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await technicians.GetByIdAsync(id, ct)) != null)
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

            RuleFor(x => x.Address).NotNull();
            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address!.Line1).NotEmpty().MaximumLength(256);
                RuleFor(x => x.Address!.City).NotEmpty().MaximumLength(128);
                RuleFor(x => x.Address!.State).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.Address!.State));
                RuleFor(x => x.Address!.PostalCode).NotEmpty().MaximumLength(32);
                RuleFor(x => x.Address!.Country).NotEmpty().MaximumLength(128);
            });
        }
    }
}
