using FinalProjectV3.DTOS;
using FluentValidation;

namespace FinalProjectV3.Validations
{
    public class LoanRequestDtoValidation : AbstractValidator<LoanRequestDto>
    {
        public LoanRequestDtoValidation()
        {
            RuleFor(x => x.LoanType)
                .IsInEnum().WithMessage("LoanType must be a valid enum value. Use 0 for Fast, 1 for Auto, or 2 for Installement.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("Currency must be a valid enum value. Use 0 for USD, 1 for EUR, or 2 for GEL.");

            RuleFor(x => x.Period)
                .IsInEnum().WithMessage("Period must be a valid enum value. Use 0 for OneMonth, 1 for ThreeMonth, or 2 for SixMonth.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be a positive value greater than 0.");
        }
    }
}
