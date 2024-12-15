namespace FinalProjectV3.DTOS
{
    public class RegistrationResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; internal set; }
        public string LastName { get; internal set; }
        public string Username { get; internal set; }
        public int Age { get; internal set; }
        public int Salary { get; internal set; }
    }
}
