using static FinalProjectV3.Models.Enums;

namespace FinalProjectV3.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public LoanType LoanType { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public Period Period { get; set; }
        public LoanStatus LoanStatus { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
