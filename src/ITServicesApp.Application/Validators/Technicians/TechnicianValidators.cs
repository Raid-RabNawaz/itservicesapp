using System.Linq;
using FluentValidation;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.Technicians
{
    public class CreateTechnicianDtoValidator : AbstractValidator<CreateTechnicianDto>
    {
        public CreateTechnicianDtoValidator(
            IUserRepository users,
            ITechnicianRepository techs,
            IServiceCategoryRepository categories)
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .MustAsync(async (userId, ct) => (await users.GetByIdAsync(userId, ct)) != null)
                .WithMessage("User does not exist.")
                .MustAsync(async (userId, ct) =>
                {
                    // Replaced techs.Query() + EF call with repository ListAsync
                    var existing = await techs.ListAsync(t => t.UserId == userId, ct);
                    return existing.Count == 0;
                })
                .WithMessage("This user is already associated with a technician profile.");

            RuleFor(x => x.ServiceCategoryId)
                .GreaterThan(0)
                .MustAsync(async (catId, ct) => (await categories.GetByIdAsync(catId, ct)) != null)
                .WithMessage("Service category does not exist.");

            RuleFor(x => x.HourlyRate).GreaterThanOrEqualTo(0).When(x => x.HourlyRate.HasValue);
            RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio != null);
        }
    }

    public class UpdateTechnicianProfileDtoValidator : AbstractValidator<UpdateTechnicianProfileDto>
    {
        public UpdateTechnicianProfileDtoValidator(IServiceCategoryRepository categories)
        {
            RuleFor(x => x.ServiceCategoryId)
                .GreaterThan(0)
                .MustAsync(async (catId, ct) => (await categories.GetByIdAsync(catId, ct)) != null)
                .WithMessage("Service category does not exist.");

            RuleFor(x => x.HourlyRate).GreaterThanOrEqualTo(0).When(x => x.HourlyRate.HasValue);
            RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio != null);
        }
    }
}
