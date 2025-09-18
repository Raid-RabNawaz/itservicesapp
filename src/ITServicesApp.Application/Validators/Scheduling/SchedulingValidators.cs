using FluentValidation;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.Scheduling
{
    public class CreateTechnicianSlotDtoValidator : AbstractValidator<CreateTechnicianSlotDto>
    {
        public CreateTechnicianSlotDtoValidator(ITechnicianRepository techs, ITechnicianSlotRepository slots)
        {
            RuleFor(x => x.TechnicianId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await techs.GetByIdAsync(id, ct)) != null)
                .WithMessage("Technician does not exist.");

            RuleFor(x => x.StartUtc).LessThan(x => x.EndUtc).WithMessage("Start must be before End.");
            RuleFor(x => x).MustAsync(async (dto, ct) =>
            {
                // Disallow overlapping slots
                var overlaps = await slots.ListAsync(s =>
                    s.TechnicianId == dto.TechnicianId &&
                    s.StartUtc < dto.EndUtc && dto.StartUtc < s.EndUtc, ct);
                return overlaps.Count == 0;
            }).WithMessage("This slot overlaps with an existing slot.");
        }
    }

    public class CreateUnavailabilityDtoValidator : AbstractValidator<CreateUnavailabilityDto>
    {
        public CreateUnavailabilityDtoValidator(ITechnicianRepository techs, ITechnicianUnavailabilityRepository unavRepo)
        {
            RuleFor(x => x.TechnicianId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await techs.GetByIdAsync(id, ct)) != null)
                .WithMessage("Technician does not exist.");

            RuleFor(x => x.StartUtc).LessThan(x => x.EndUtc).WithMessage("Start must be before End.");
            RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason != null);

            RuleFor(x => x).MustAsync(async (dto, ct) =>
                    !(await unavRepo.HasOverlapAsync(dto.TechnicianId, dto.StartUtc, dto.EndUtc, ct)))
                .WithMessage("This unavailability overlaps with an existing one.");
        }
    }
}
