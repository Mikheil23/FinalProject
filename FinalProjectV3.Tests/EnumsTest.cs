using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectV3.Tests
{
    public static class Enums
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
            Denied
        }
    }
}
