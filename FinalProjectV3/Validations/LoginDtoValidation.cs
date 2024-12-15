using FinalProjectV3.DTOS;
using FluentValidation;

namespace FinalProjectV3.Validations
{
    public class LoginDtoValidation : AbstractValidator<LoginDto>
    {
        public LoginDtoValidation()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(6).WithMessage("Username must be at least 6 characters long.")
                .Matches(@"^\S.*\S$").WithMessage("Username must not have leading or trailing spaces.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"^\S.*\S$").WithMessage("Password must not have leading or trailing spaces.");
        }
    }

}
