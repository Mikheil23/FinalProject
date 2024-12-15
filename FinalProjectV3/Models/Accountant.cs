namespace FinalProjectV3.Models
{
    public class Accountant
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } = Roles.Accountant;
        public int? AccountantCredentialsId { get; set; } 
        public AccountantCredentials AccountantCredentials { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
