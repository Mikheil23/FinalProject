using FinalProjectV3.Context;
using FinalProjectV3.DTOS;
using FinalProjectV3.Helpers;
using FinalProjectV3.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinalProjectV3.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;
        private readonly ValidationHelper _validationHelper;
        private readonly ILogger<AuthService> _logger;
        public AuthService(AppDbContext context, IConfiguration configuration, IOptions<AppSettings> appSettings, ValidationHelper validationHelper, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _appSettings = appSettings.Value;
            _validationHelper = validationHelper;
            _logger = logger;

        }
        public async Task<RegistrationResponse> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Started registration for user with email: {Email}", registerDto.Email);
            try
            {
                await _validationHelper.ValidateAsync(registerDto);

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Email already in use for {Email}.", registerDto.Email);
                    throw new Exception("Email already in use.");
                }
                var existingCredentials = await _context.Credentials
                .FirstOrDefaultAsync(c => c.UserName == registerDto.Username);
            if (existingCredentials != null)
                {
                    _logger.LogWarning("Registration failed: Username already in use for {Username}.", registerDto.Username);
                    throw new Exception("Username already in use.");
                }
                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_configuration["AppSettings:Secret"]))) 
                {
                    var credentials = new Credentials
                    {
                        UserName = registerDto.Username,
                        Password = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDto.Password))) 
                    };


                    _context.Credentials.Add(credentials);
                    await _context.SaveChangesAsync();

                    var user = new User
                    {
                        FirstName = registerDto.FirstName,
                        LastName = registerDto.LastName,
                        Age = registerDto.Age,
                        Email = registerDto.Email,
                        Salary = registerDto.Salary ?? 0,
                        IsBlocked = false, 
                        CredentialsId = credentials.Id, 
                        Loans = new List<Loan>(), 
                        Role = Roles.User
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(); 
                    _logger.LogInformation("User registration successful for {Email}.", registerDto.Email);
                    return new RegistrationResponse
                    {
                        UserId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Username = credentials.UserName,
                        Age = user.Age,
                        Salary = user.Salary
                    };
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for email: {Email}.", registerDto.Email);
                throw;  
            }

        }
        public async Task<AccountantRegistrationResponse> RegisterAccountantAsync(AccountantRegisterDto accountantregisterDto)
        {
            _logger.LogInformation("Started accountant registration for username: {UserName}", accountantregisterDto.UserName);
            try
            {
                await _validationHelper.ValidateAsync(accountantregisterDto);
                var existingAccountant = await _context.Accountants
                    .FirstOrDefaultAsync(a => a.AccountantCredentials.UserName == accountantregisterDto.UserName);
                if (existingAccountant != null)
                {
                    _logger.LogWarning("Registration failed: Username already in use for {UserName}.", accountantregisterDto.UserName);
                    throw new Exception("Username already in use.");
                }
                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_configuration["AppSettings:Secret"])))
                {
                    var accountantCredentials = new AccountantCredentials
                    {
                        UserName = accountantregisterDto.UserName,
                        Password = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(accountantregisterDto.Password))) 
                    };

                    _context.AccountantCredentials.Add(accountantCredentials);
                    await _context.SaveChangesAsync(); 

                    var accountant = new Accountant
                    {
                        FirstName = accountantregisterDto.FirstName,
                        LastName = accountantregisterDto.LastName,
                        AccountantCredentialsId = accountantCredentials.Id,
                        Role = Roles.Accountant
                    };


                    _context.Accountants.Add(accountant);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Accountant successfully registered with username: {UserName}.", accountantregisterDto.UserName);
                    return new AccountantRegistrationResponse
                    {
                        AccountantId = accountant.Id,
                        FirstName = accountant.FirstName,
                        LastName = accountant.LastName,
                        UserName = accountantCredentials.UserName
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during accountant registration for username: {UserName}.", accountantregisterDto.UserName);
                throw;
            }
        }
        public async Task<LoginResponse> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt started for username: {Username}", loginDto.Username);
            await _validationHelper.ValidateAsync(loginDto);
            var user = await _context.Users
                .Include(u => u.Credentials) 
                .FirstOrDefaultAsync(u => u.Credentials.UserName == loginDto.Username);

            var accountant = await _context.Accountants
                .Include(a => a.AccountantCredentials) 
                .FirstOrDefaultAsync(a => a.AccountantCredentials.UserName == loginDto.Username);
            if (user == null && accountant == null)
            {
                _logger.LogWarning("Invalid username or password for username: {Username}.", loginDto.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            if (user != null)
            {
                

                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_configuration["AppSettings:Secret"]))) 
                {
                    var computedHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDto.Password)));
                   

                    if (user.Credentials.Password != computedHash)
                    {
                        _logger.LogWarning("Invalid password for username: {Username}.", loginDto.Username);
                        throw new UnauthorizedAccessException("Invalid username or password.");
                    }
                }
            }

            if (accountant != null)
            {
                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_configuration["AppSettings:Secret"])))
                {
                    var computedHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDto.Password)));
                    if (accountant.AccountantCredentials.Password != computedHash)
                    {
                        _logger.LogWarning("Invalid password for username: {Username}.", loginDto.Username);
                        throw new UnauthorizedAccessException("Invalid username or password.");
                    }
                }
            }

            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.Name, loginDto.Username),
            new Claim(ClaimTypes.NameIdentifier, user?.Id.ToString() ?? accountant?.Id.ToString() ?? string.Empty) 
            };

            string role = user?.Role ?? accountant?.Role ?? "User";

            claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            _logger.LogInformation("Login successful for username: {Username}. Token generated.", loginDto.Username);

            return new LoginResponse { Token = tokenString };

        }
    }
}

