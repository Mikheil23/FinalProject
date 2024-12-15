using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FinalProjectV3.Services;
using FinalProjectV3.DTOS;
using FinalProjectV3.Models;
using FinalProjectV3.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;
using FinalProjectV3.Context;
using FluentValidation;
using FinalProjectV3.Validations;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly AppDbContext _context;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase") 
            .Options;

        _context = new AppDbContext(options); 

        _context.Database.EnsureDeleted(); 
        _context.Database.EnsureCreated();
        var inMemorySettings = new Dictionary<string, string>
        {
            { "AppSettings:Secret", "a-very-secure-long-secret-key-for-jwt-that-is-128-bits" } 
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings) 
            .Build();

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthService>();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<ILogger<AuthService>>(logger)
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IAccountantService, AccountantService>()
            .AddScoped<IUserService, UserService>()
            .AddValidatorsFromAssemblyContaining<RegisterDtoValidation>() 
            .AddValidatorsFromAssemblyContaining<AccountantRegisterDtoValidation>()
            .AddValidatorsFromAssemblyContaining<LoginDtoValidation>()
            .AddValidatorsFromAssemblyContaining<LoanRequestDtoValidation>()
            .AddTransient<ValidationHelper>() 
            .BuildServiceProvider(); 

        var validationHelper = serviceProvider.GetRequiredService<ValidationHelper>();

        var appSettings = Options.Create(new AppSettings { Secret = "a-very-secure-long-secret-key-for-jwt-that-is-128-bits" });

        _authService = new AuthService(_context, configuration, appSettings, validationHelper, logger);
    }



    [Fact]
    public async Task RegisterAsync_ShouldReturnRegistrationResponse_WhenRegistrationIsSuccessful()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "john.doe@example.com",
            Username = "johndoe123",
            Password = "Password123!",
            Salary = 50000
        };

        var result = await _authService.RegisterAsync(registerDto);

        Assert.NotNull(result);
        Assert.Equal(registerDto.FirstName, result.FirstName);
        Assert.Equal(registerDto.LastName, result.LastName);
        Assert.Equal(registerDto.Email, result.Email);
        Assert.Equal(registerDto.Username, result.Username);
    }
    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        var existingUser = new User
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@example.com",
            Age = 25,
            Credentials = new Credentials { UserName = "existinguser", Password = "ExistingPass123!" },
            Salary = 40000
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var duplicateEmailDto = new RegisterDto
        {
            FirstName = "New",
            LastName = "User",
            Age = 30,
            Email = "existing@example.com",
            Username = "newuser123",
            Password = "NewPass123!",
            Salary = 60000
        };

        var exception = await Assert.ThrowsAsync<System.Exception>(() =>
            _authService.RegisterAsync(duplicateEmailDto));

        Assert.Equal("Email already in use.", exception.Message);
    }
    [Fact]
    public async Task RegisterAccountantAsync_ShouldThrowException_WhenPasswordIsWeak()
    {
        var weakPasswordDto = new AccountantRegisterDto
        {
            UserName = "validUser123",
            Password = "123",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _authService.RegisterAccountantAsync(weakPasswordDto));

        Assert.Contains("Password must contain at least one uppercase letter.", exception.Message);
        Assert.Contains("Password must contain at least one lowercase letter.", exception.Message);
        Assert.Contains("Password must contain at least one symbol.", exception.Message);
        Assert.Contains("Password must be at least 8 characters long.", exception.Message);
    }



    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenValidationFails()
    {
        var invalidRegisterDto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "invalidemail", 
            Username = "johndoe123",
            Password = "Password123!",
            Salary = 50000
        };

        await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _authService.RegisterAsync(invalidRegisterDto);
        });
    }
   


    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenLoginIsSuccessful()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "john.doe@example.com",
            Username = "johndoe123",
            Password = "Password123!",
            Salary = 50000
        };
        var registerResult = await _authService.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Username = registerDto.Username,
            Password = registerDto.Password
        };

        var loginResult = await _authService.LoginAsync(loginDto);

        Assert.NotNull(loginResult.Token);
    }
    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenAccountantLogsInSuccessfully()
    {
        var accountantDto = new AccountantRegisterDto
        {
            UserName = "accountant123",
            Password = "ValidPassword123!",
            FirstName = "Alice",
            LastName = "Doe"
        };
        await _authService.RegisterAccountantAsync(accountantDto);

        var loginDto = new LoginDto
        {
            Username = "accountant123",
            Password = "ValidPassword123!"
        };

        var result = await _authService.LoginAsync(loginDto);

        Assert.NotNull(result.Token);
    }
    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenPasswordIsIncorrect()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "john.doe@example.com",
            Username = "johndoe123",
            Password = "Password123!",
            Salary = 50000
        };
        await _authService.RegisterAsync(registerDto);

        var invalidLoginDto = new LoginDto
        {
            Username = "johndoe123",
            Password = "WrongPassword123!"
        };

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(invalidLoginDto));

        Assert.Equal("Invalid username or password.", exception.Message);
    }


    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        var loginDto = new LoginDto
        {
            Username = "nonexistentuser",
            Password = "somepassword"
        };

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(loginDto));

        Assert.Equal("Invalid username or password.", exception.Message);
    }


    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenInvalidCredentials()
    {
        var loginDto = new LoginDto
        {
            Username = "nonexistentuser",
            Password = "wrongpassword"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _authService.LoginAsync(loginDto);
        });
    }
    [Fact]
    public async Task RegisterAccountantAsync_ShouldReturnResponse_WhenRegistrationIsSuccessful()
    {
        var validRegisterDto = new AccountantRegisterDto
        {
            UserName = "validUser123",  
            Password = "ValidPassword123!",  
            FirstName = "Jane",
            LastName = "Doe"
        };

        var validator = new AccountantRegisterDtoValidation(); 
        var validationResult = await validator.ValidateAsync(validRegisterDto);

        Assert.True(validationResult.IsValid);

        var existingAccountant = await _context.Accountants
            .FirstOrDefaultAsync(a => a.AccountantCredentials.UserName == validRegisterDto.UserName);

        if (existingAccountant != null)
        {
            _context.Accountants.Remove(existingAccountant);
            await _context.SaveChangesAsync();
        }

        var result = await _authService.RegisterAccountantAsync(validRegisterDto);

        Assert.NotNull(result);

        Assert.Equal(validRegisterDto.FirstName, result.FirstName);
        Assert.Equal(validRegisterDto.LastName, result.LastName);
        Assert.Equal(validRegisterDto.UserName, result.UserName);
    }


    [Fact]
    public async Task RegisterAccountantAsync_ShouldThrowException_WhenUsernameAlreadyExists()
    {
        var validUsernameDto = new AccountantRegisterDto
        {
            UserName = "validUser123",  
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var existingAccountant = new Accountant
        {
            FirstName = "John",
            LastName = "Doe",
            AccountantCredentials = new AccountantCredentials
            {
                UserName = "validUser123",  
                Password = "ValidPassword123!"
            }
        };
        _context.Accountants.Add(existingAccountant);
        await _context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<System.Exception>(() => _authService.RegisterAccountantAsync(validUsernameDto));

        Assert.Equal("Username already in use.", exception.Message);
    }





    [Fact]
    public async Task RegisterAccountantAsync_ShouldThrowValidationException_WhenDtoIsInvalid()
    {
        var invalidRegisterDto = new AccountantRegisterDto
        {
            UserName = "123",  
            Password = "123",  
            FirstName = "Jane",
            LastName = "Doe"
        };

        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _authService.RegisterAccountantAsync(invalidRegisterDto));

        Assert.Contains("Username must be at least 6 characters long.", exception.Message);
        Assert.Contains("Password must contain at least one uppercase letter.", exception.Message);
        Assert.Contains("Password must contain at least one lowercase letter.", exception.Message);
        Assert.Contains("Password must contain at least one symbol.", exception.Message);
        Assert.Contains("Password must be at least 8 characters long.", exception.Message);
    }



}



