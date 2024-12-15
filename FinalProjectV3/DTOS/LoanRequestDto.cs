using static FinalProjectV3.Models.Enums;

namespace FinalProjectV3.DTOS
{
    public class LoanRequestDto
    {
        public LoanType LoanType { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public Period Period { get; set; }
    }
}
