using FluentValidation;

namespace SaaS.Application.Commands.Auth;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Request.TenantName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(100).WithMessage("Company name must not exceed 100 characters");

        RuleFor(x => x.Request.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters")
            .MaximumLength(63).WithMessage("Subdomain must not exceed 63 characters");
    }
}
