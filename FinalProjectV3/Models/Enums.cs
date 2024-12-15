namespace FinalProjectV3.Models
{
    public class Enums
    {
        public enum LoanType
        {
            Fast,
            Auto,
            Installement
        }

        public enum Currency
        {
            USD,
            EUR,
            GEL
        }

        public enum Period
        {
            OneMonth,
            ThreeMonth,
            SixMonth
        }

        public enum LoanStatus
        {
            InProgress,
            Approved,
            Denied,
        }
    }
}
