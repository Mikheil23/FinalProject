using FinalProjectV3.DTOS;
using FluentValidation;

namespace FinalProjectV3.Validations
{
    public class RegisterDtoValidation : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidation()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .Matches(@"^\S.*\S$").WithMessage("First name must not have leading or trailing spaces.")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters long.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .Matches(@"^\S.*\S$").WithMessage("Last name must not have leading or trailing spaces.")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters long.");

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18).WithMessage("Age must be 18 or older.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative.");

            RuleFor(x => x.Username)
                .MinimumLength(6).WithMessage("Username must be at least 6 characters long.")
                .Matches(@"\d").WithMessage("Username must contain at least one number.");

            RuleFor(x => x.Password)
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one number.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one symbol.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
        }
    }
}
