namespace FinalProjectV3.Models
{
    public class AccountantCredentials
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Accountant Accountant { get; set; }
    }
}
