using FinalProjectV3.DTOS;

namespace FinalProjectV3.Services
{
    public interface IAuthService
    {
        Task<RegistrationResponse> RegisterAsync(RegisterDto registerDto);
        Task<AccountantRegistrationResponse> RegisterAccountantAsync(AccountantRegisterDto accountantregisterDto);
        Task<LoginResponse> LoginAsync(LoginDto loginDto);
    }
}
