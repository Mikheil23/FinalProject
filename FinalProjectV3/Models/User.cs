
namespace FinalProjectV3.Models

{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? CredentialsId { get; set; }
        public Credentials Credentials { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public int Salary { get; set; }
        public bool IsBlocked { get; set; } = false;
        public string Role { get; set; } = Roles.User;
        public ICollection<Loan> Loans { get; set; }
        public int? AccountantId { get; set; } 
        public Accountant Accountant { get; set; }
       
    }
}
