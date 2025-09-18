using FluentValidation;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.ServiceCatalog
{
    public class CreateServiceCategoryDtoValidator : AbstractValidator<CreateServiceCategoryDto>
    {
        public CreateServiceCategoryDtoValidator(IServiceCategoryRepository categories)
        {
            RuleFor(x => x.Name)
                .NotEmpty().MaximumLength(200)
                .MustAsync(async (name, ct) =>
                {
                    var existing = await categories.ListAsync(c => c.Name == name, ct);
                    return existing.Count == 0;
                }).WithMessage("A category with the same name already exists.");

            RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
        }
    }

    public class UpdateServiceCategoryDtoValidator : AbstractValidator<UpdateServiceCategoryDto>
    {
        public UpdateServiceCategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
        }
    }

    public class CreateServiceIssueDtoValidator : AbstractValidator<CreateServiceIssueDto>
    {
        public CreateServiceIssueDtoValidator(IServiceCategoryRepository categories, IServiceIssueRepository issues)
        {
            RuleFor(x => x.ServiceCategoryId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await categories.GetByIdAsync(id, ct)) != null)
                .WithMessage("Service category does not exist.");

            RuleFor(x => x.Name)
                .NotEmpty().MaximumLength(200)
                .MustAsync(async (dto, name, ct) =>
                {
                    var dup = await issues.ListAsync(i => i.ServiceCategoryId == dto.ServiceCategoryId && i.Name == name, ct);
                    return dup.Count == 0;
                })
                .WithMessage("An issue with the same name already exists in this category.");

            RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
            RuleFor(x => x.EstimatedDurationMinutes)
                .GreaterThan(0).When(x => x.EstimatedDurationMinutes.HasValue);
            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateServiceIssueDtoValidator : AbstractValidator<UpdateServiceIssueDto>
    {
        public UpdateServiceIssueDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
            RuleFor(x => x.EstimatedDurationMinutes)
                .GreaterThan(0).When(x => x.EstimatedDurationMinutes.HasValue);
            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0);
            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0);
        }
    }
}

