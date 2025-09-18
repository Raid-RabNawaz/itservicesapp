using System;
using FluentValidation;
using ITServicesApp.Application.UseCases.Bookings.Pipeline;

namespace ITServicesApp.Application.Validators.Bookings
{
    public sealed class StartBookingDraftCommandValidator : AbstractValidator<StartBookingDraftCommand>
    {
        public StartBookingDraftCommandValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            RuleFor(x => x.Dto.ServiceIssueId).GreaterThan(0);
            RuleFor(x => x.Dto.ServiceCategoryId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Dto.Email)
                .MaximumLength(256)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
            RuleFor(x => x.Dto.FullName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.FullName));
            RuleForEach(x => x.Dto.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.ServiceIssueId).GreaterThan(0);
                    item.RuleFor(i => i.Quantity).GreaterThan(0);
                });
        }
    }

    public sealed class UpdateBookingDraftAddressCommandValidator : AbstractValidator<UpdateBookingDraftAddressCommand>
    {
        public UpdateBookingDraftAddressCommandValidator()
        {
            RuleFor(x => x.DraftId).NotEqual(Guid.Empty);
            RuleFor(x => x.Address.Line1).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Address.City).NotEmpty().MaximumLength(128);
            RuleFor(x => x.Address.State).MaximumLength(128);
            RuleFor(x => x.Address.PostalCode).NotEmpty().MaximumLength(32);
            RuleFor(x => x.Address.Country).NotEmpty().MaximumLength(128);
            RuleFor(x => x.Address.Notes).MaximumLength(1000);
        }
    }

    public sealed class SelectBookingDraftSlotCommandValidator : AbstractValidator<SelectBookingDraftSlotCommand>
    {
        public SelectBookingDraftSlotCommandValidator()
        {
            RuleFor(x => x.DraftId).NotEqual(Guid.Empty);
            RuleFor(x => x.Slot.StartUtc).NotEqual(default(DateTime));
            RuleFor(x => x.Slot)
                .Must(s => !s.EndUtc.HasValue || s.EndUtc.Value > s.StartUtc)
                .WithMessage("End time must be after start time.");
            RuleFor(x => x.Slot.DurationMinutes)
                .GreaterThan(0)
                .When(x => x.Slot.DurationMinutes.HasValue);
        }
    }

    public sealed class ConfirmBookingDraftCommandValidator : AbstractValidator<ConfirmBookingDraftCommand>
    {
        public ConfirmBookingDraftCommandValidator()
        {
            RuleFor(x => x.DraftId).NotEqual(Guid.Empty);
        }
    }
}
