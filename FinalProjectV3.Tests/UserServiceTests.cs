using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FinalProjectV3.Context;
using FinalProjectV3.Services;
using FinalProjectV3.Helpers;
using FinalProjectV3.Models;
using FluentValidation;
using FinalProjectV3.DTOS;
using static FinalProjectV3.Models.Enums;
using Moq;

public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly AppDbContext _context;

    public UserServiceTests()
    {
        var configuration = BuildConfiguration();
        _context = BuildInMemoryDbContext();
        var logger = BuildLogger<UserService>();
        var validationHelper = BuildValidationHelper(configuration);

        _userService = new UserService(_context, configuration, validationHelper, logger);
    }

    private IConfiguration BuildConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "AppSettings:Secret", "a-very-secure-long-secret-key-for-jwt-that-is-128-bits" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private AppDbContext BuildInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }

    private ILogger<T> BuildLogger<T>()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }).CreateLogger<T>();
    }

    private ValidationHelper BuildValidationHelper(IConfiguration configuration)
    {
        var serviceProvider = new ServiceCollection()
            .AddValidatorsFromAssemblyContaining<ValidationHelper>()
            .BuildServiceProvider();

        return new ValidationHelper(serviceProvider);
    }
    [Fact]
    public async Task AddLoanRequestAsync_Should_Add_Loan_When_Valid_Request()
    {
        var userId = 1;
        var role = "User";
        var loggedInUserId = "1";
        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Auto,
            Amount = 1000,
            Currency = Currency.GEL,
            Period = Period.ThreeMonth
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com", 
            FirstName = "John",        
            LastName = "Doe",          
            IsBlocked = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _userService.AddLoanRequestAsync(loanRequestDto, userId, role, loggedInUserId);

        Assert.NotNull(result);
        Assert.Equal(loanRequestDto.LoanType, result.LoanType);
        Assert.Equal(loanRequestDto.Amount, result.Amount);
        Assert.Equal(LoanStatus.InProgress, result.LoanStatus);
    }
    [Fact]
    public async Task AddLoanRequestAsync_Should_Throw_UnauthorizedAccessException_When_Role_Is_Not_User()
    {
        var userId = 1;
        var role = "Admin"; 
        var loggedInUserId = "1";
        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Installement,
            Amount = 1000,
            Currency = Currency.EUR,
            Period = Period.ThreeMonth,
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsBlocked = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.AddLoanRequestAsync(loanRequestDto, userId, role, loggedInUserId)
        );
        Assert.Equal("User does not have sufficient permissions.", exception.Message);
    }
    [Fact]
    public async Task AddLoanRequestAsync_Should_Throw_UnauthorizedAccessException_When_UserId_Is_Not_Matching()
    {
        var userId = 1;
        var role = "User";
        var loggedInUserId = "2"; 
        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Installement,
            Amount = 1000,
            Currency = Currency.GEL,
            Period = Period.OneMonth,
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsBlocked = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.AddLoanRequestAsync(loanRequestDto, userId, role, loggedInUserId)
        );
        Assert.Equal("You can only add loans for your own account.", exception.Message);
    }
    [Fact]
    public async Task AddLoanRequestAsync_Should_Throw_UnauthorizedAccessException_When_User_Is_Blocked()
    {
        var userId = 1;
        var role = "User";
        var loggedInUserId = "1";
        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Installement,
            Amount = 1000,
            Currency = Currency.GEL,
            Period = Period.OneMonth,
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsBlocked = true 
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.AddLoanRequestAsync(loanRequestDto, userId, role, loggedInUserId)
        );
        Assert.Equal("Invalid user or user is blocked.", exception.Message);
    }
    [Fact]
    public async Task ViewUserCabinetByUserIdAsync_Should_Return_User_Cabinet_When_Valid_UserId()
    {
        var userId = 1;
        var loggedInUserId = "1";

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Age = 30,
            Salary = 50000,
            Credentials = new Credentials
            {
                UserName = "johndoe",
                Password = "SecurePassword123" 
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = await _userService.ViewUserCabinetByUserIdAsync(userId, loggedInUserId);

        Assert.NotNull(response);
        Assert.Equal(userId, response.UserId);
        Assert.Equal("John", response.FirstName);
        Assert.Equal("Doe", response.LastName);
        Assert.Equal("john.doe@example.com", response.Email);
        Assert.Equal(30, response.Age);
        Assert.Equal(50000, response.Salary);
        Assert.Equal("johndoe", response.Username);
    }
    [Fact]
    public async Task ViewUserCabinetByUserIdAsync_Should_Throw_UnauthorizedAccessException_When_User_Tries_To_Access_Other_Cabinet()
    {
        var userId = 1;
        var loggedInUserId = "2"; 

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.ViewUserCabinetByUserIdAsync(userId, loggedInUserId)
        );
        Assert.Contains("You can only view your own cabinet.", exception.Message);
    }
    [Fact]
    public async Task ViewUserCabinetByUserIdAsync_Should_Throw_KeyNotFoundException_When_User_Not_Found()
    {
        var userId = 999; 
        var loggedInUserId = "999";

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _userService.ViewUserCabinetByUserIdAsync(userId, loggedInUserId)
        );
        Assert.Contains("User not found.", exception.Message);
    }
    [Fact]
    public async Task ViewAllLoansAsync_Should_Return_All_Loans_When_Valid_UserId()
    {
        var userId = 1;
        var loggedInUserId = "1";

        var loan1 = new Loan
        {
            UserId = userId,
            LoanType = LoanType.Installement,
            Amount = 1000,
            Currency = Currency.GEL,
            Period = Period.OneMonth,
            LoanStatus = LoanStatus.InProgress
        };
        var loan2 = new Loan
        {
            UserId = userId,
            LoanType = LoanType.Auto,
            Amount = 50000,
            Currency = Currency.USD,
            Period = Period.SixMonth,
            LoanStatus = LoanStatus.Approved
        };

        _context.Loans.AddRange(loan1, loan2);
        await _context.SaveChangesAsync();

        var loanResponses = await _userService.ViewAllLoansAsync(userId, loggedInUserId);

        Assert.NotNull(loanResponses);
        Assert.Equal(2, loanResponses.Count());
        Assert.Contains(loanResponses, loan => loan.LoanType == LoanType.Installement);
        Assert.Contains(loanResponses, loan => loan.LoanType == LoanType.Auto);
    }
    [Fact]
    public async Task ViewAllLoansAsync_Should_Throw_KeyNotFoundException_When_No_Loans_Found()
    {
        var userId = 1;
        var loggedInUserId = "1"; 

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _userService.ViewAllLoansAsync(userId, loggedInUserId)
        );
        Assert.Contains("No loans found for this user.", exception.Message);
    }
    [Fact]
    public async Task ViewAllLoansAsync_Should_Throw_UnauthorizedAccessException_When_User_Tries_To_View_Other_Loans()
    {
        var userId = 1;
        var loggedInUserId = "2"; 

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.ViewAllLoansAsync(userId, loggedInUserId)
        );
        Assert.Contains("You can only view your own loans.", exception.Message);
    }
    [Fact]
    public async Task UpdateLoanAsync_Should_Update_Loan_When_Valid_UserId_And_Valid_Loan()
    {
        var userId = 1;
        var loggedInUserId = "1"; 
        var loanId = 1;

        var user = new User
        {
            Id = userId,
            FirstName = "John",  
            LastName = "Doe",
            Email = "john.doe@example.com",
            IsBlocked = false 
        };

        _context.Users.Add(user);

        var loan = new Loan
        {
            Id = loanId,
            UserId = userId,
            LoanType = LoanType.Installement,
            Amount = 1000,
            Currency = Currency.GEL,
            Period = Period.OneMonth,
            LoanStatus = LoanStatus.InProgress
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(); 

        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Auto,
            Amount = 20000,
            Currency = Currency.USD,
            Period = Period.SixMonth
        };

        var updatedLoan = await _userService.UpdateLoanAsync(loanId, loanRequestDto, userId, "User", loggedInUserId);

        Assert.NotNull(updatedLoan);
        Assert.Equal(loanId, updatedLoan.LoanId);
        Assert.Equal(LoanType.Auto, updatedLoan.LoanType);
        Assert.Equal(20000, updatedLoan.Amount);
        Assert.Equal(Currency.USD, updatedLoan.Currency);
        Assert.Equal(Period.SixMonth, updatedLoan.Period);
    }



    [Fact]
    public async Task UpdateLoanAsync_Should_Throw_UnauthorizedAccessException_When_User_With_Another_Role_Tries_To_Update_Loan()
    {
        var userId = 1;
        var loggedInUserId = "1";
        var loanId = 1;
        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Auto,
            Amount = 20000,
            Currency = Currency.USD,
            Period = Period.SixMonth
        };

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.UpdateLoanAsync(loanId, loanRequestDto, userId, "Admin", loggedInUserId)
        );
        Assert.Contains("Only users can update loans.", exception.Message);
    }

    [Fact]
    public async Task UpdateLoanAsync_Should_Throw_UnauthorizedAccessException_When_User_Tries_To_Update_Another_Users_Loan()
    {
        var userId = 1;
        var loggedInUserId = "2"; 
        var loanId = 1;
        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Auto,
            Amount = 20000,
            Currency = Currency.USD,
            Period = Period.SixMonth
        };

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.UpdateLoanAsync(loanId, loanRequestDto, userId, "User", loggedInUserId)
        );
        Assert.Contains("You can only update your own loans.", exception.Message);
    }

    [Fact]
    public async Task UpdateLoanAsync_Should_Throw_UnauthorizedAccessException_When_User_Is_Blocked()
    {
        var userId = 1;
        var loggedInUserId = "1"; 
        var loanId = 1;

        var user = new User
        {
            Id = userId,
            FirstName = "John",  
            LastName = "Doe",
            Email = "john.doe@example.com",
            IsBlocked = true 
        };

        _context.Users.Add(user);

        var loan = new Loan
        {
            Id = loanId,
            UserId = userId,
            LoanType = LoanType.Installement,
            Amount = 1000,
            Currency = Currency.GEL,
            Period = Period.OneMonth,
            LoanStatus = LoanStatus.InProgress
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(); 

        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Auto,
            Amount = 20000,
            Currency = Currency.USD,
            Period = Period.SixMonth
        };

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.UpdateLoanAsync(loanId, loanRequestDto, userId, "User", loggedInUserId)
        );

        Assert.Contains("Invalid user or user is blocked.", exception.Message);
    }


    [Fact]
    public async Task UpdateLoanAsync_Should_Throw_KeyNotFoundException_When_Loan_Not_Found()
    {
        var userId = 1;
        var loggedInUserId = "1"; 
        var loanId = 999;

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            IsBlocked = false 
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Auto,
            Amount = 20000,
            Currency = Currency.USD,
            Period = Period.SixMonth
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _userService.UpdateLoanAsync(loanId, loanRequestDto, userId, "User", loggedInUserId)
        );

        Assert.Contains("Loan not found.", exception.Message);
    }


    [Fact]
    public async Task UpdateLoanAsync_Should_Throw_InvalidOperationException_When_Loan_Status_Is_Not_InProgress()
    {
        var userId = 1;
        var loggedInUserId = "1"; 
        var loanId = 123; 
        var loanStatus = LoanStatus.Approved; 

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            IsBlocked = false 
        };

        _context.Users.Add(user);

        var loan = new Loan
        {
            Id = loanId,
            UserId = userId,
            LoanType = LoanType.Auto,
            Amount = 10000,
            Currency = Currency.USD,
            Period = Period.SixMonth,
            LoanStatus = loanStatus
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        var loanRequestDto = new LoanRequestDto
        {
            LoanType = LoanType.Auto,
            Amount = 20000,
            Currency = Currency.USD,
            Period = Period.OneMonth
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.UpdateLoanAsync(loanId, loanRequestDto, userId, "User", loggedInUserId)
        );

        Assert.Contains("You can only update loans that are in progress.", exception.Message);
    }
    [Fact]
    public async Task DeleteLoanAsync_Should_Throw_UnauthorizedAccessException_When_Role_Is_Not_User()
    {
        var userId = 1;
        var loanId = 123;
        var role = "Admin";  
        var loggedInUserId = "1";

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.DeleteLoanAsync(userId, loanId, role, loggedInUserId)
        );

        Assert.Equal("Only users can delete loans.", exception.Message);
    }
    [Fact]
    public async Task DeleteLoanAsync_Should_Throw_UnauthorizedAccessException_When_LoggedInUserId_Is_Different()
    {
        var userId = 1;
        var loanId = 123;
        var role = "User";  
        var loggedInUserId = "2";  

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.DeleteLoanAsync(userId, loanId, role, loggedInUserId)
        );

        Assert.Equal("You can only delete your own loans.", exception.Message);
    }
    [Fact]
    public async Task DeleteLoanAsync_Should_Throw_UnauthorizedAccessException_When_User_Is_Invalid_Or_Blocked()
    {
        var userId = 1;
        var loanId = 123;
        var role = "User";  
        var loggedInUserId = "1";

        var user = new User
        {
            Id = userId,
            IsBlocked = true,
            Email = "testuser@example.com", 
            FirstName = "John",              
            LastName = "Doe"                 
        };
        _context.Users.Add(user);

        var loan = new Loan { Id = loanId, UserId = userId, LoanStatus = LoanStatus.InProgress };
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.DeleteLoanAsync(userId, loanId, role, loggedInUserId)
        );

        Assert.Equal("Invalid user or user is blocked.", exception.Message);
    }
    [Fact]
    public async Task DeleteLoanAsync_Should_Throw_KeyNotFoundException_When_Loan_Not_Found()
    {
        var userId = 1;
        var loanId = 123;
        var role = "User";  
        var loggedInUserId = "1";

        var user = new User
        {
            Id = userId,
            Email = "testuser@example.com", 
            FirstName = "John",              
            LastName = "Doe",                
            IsBlocked = false                
        };
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _userService.DeleteLoanAsync(userId, loanId, role, loggedInUserId)
        );

        Assert.Equal("Loan not found.", exception.Message);
    }
    [Fact]
    public async Task DeleteLoanAsync_Should_Throw_InvalidOperationException_When_Loan_Status_Is_Not_InProgress()
    {
        var userId = 1;
        var loanId = 123;
        var role = "User"; 
        var loggedInUserId = "1";

        var user = new User
        {
            Id = userId,
            Email = "testuser@example.com",  
            FirstName = "John",              
            LastName = "Doe",                
            IsBlocked = false                
        };

        var loan = new Loan
        {
            Id = loanId,
            UserId = userId,
            LoanStatus = LoanStatus.Approved, 
            LoanType = LoanType.Auto,
            Amount = 1000,
            Currency = Currency.USD,
            Period = Period.OneMonth,
        };

        _context.Users.Add(user);
        _context.Loans.Add(loan);

        await _context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.DeleteLoanAsync(userId, loanId, role, loggedInUserId)
        );

        Assert.Equal("You can only delete loans that are in progress.", exception.Message);
    }
    [Fact]
    public async Task DeleteLoanAsync_Should_Delete_Loan_Successfully_When_Valid_Conditions()
    {
        var userId = 1;
        var loanId = 123;
        var role = "User";  
        var loggedInUserId = "1";

        var user = new User
        {
            Id = userId,
            Email = "testuser@example.com",  
            FirstName = "John",              
            LastName = "Doe",                
            IsBlocked = false                
        };

        var loan = new Loan
        {
            Id = loanId,
            UserId = userId,
            LoanStatus = LoanStatus.InProgress, 
            LoanType = LoanType.Fast,
            Amount = 1000,
            Currency = Currency.GEL,
            Period = Period.OneMonth,
        };

        _context.Users.Add(user);
        _context.Loans.Add(loan);

        await _context.SaveChangesAsync();

        var result = await _userService.DeleteLoanAsync(userId, loanId, role, loggedInUserId);

        Assert.True(result); 

        var deletedLoan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId);
        Assert.Null(deletedLoan); 
    }




















}



