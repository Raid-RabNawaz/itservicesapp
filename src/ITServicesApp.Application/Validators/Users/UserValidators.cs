using FluentValidation;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.Users
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator(IUserRepository users)
        {
            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress().MaximumLength(256)
                .MustAsync(async (email, ct) => !(await users.EmailExistsAsync(email, ct)))
                .WithMessage("Email is already in use.");
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        }
    }

    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        }
    }

    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator(IUserRepository users)
        {
            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress().MaximumLength(256)
                .MustAsync(async (email, ct) => !(await users.EmailExistsAsync(email, ct)))
                .WithMessage("Email is already in use.");
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Password)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.");
        }
    }

    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("New password must contain at least one number.");
        }
    }

    public class FirstLoginPasswordSetupDtoValidator : AbstractValidator<FirstLoginPasswordSetupDto>
    {
        public FirstLoginPasswordSetupDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.");
        }
    }
}
