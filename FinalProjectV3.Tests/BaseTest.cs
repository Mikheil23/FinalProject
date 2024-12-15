using FinalProjectV3.Context;
using FinalProjectV3.Models;
using FinalProjectV3.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;

namespace FinalProjectV3.Tests
{
    public class BaseTest : IDisposable
    {
        protected readonly AppDbContext _context;
        protected readonly Mock<IConfiguration> _mockConfiguration;
        protected readonly Mock<ILogger<AccountantService>> _mockLogger;
        protected readonly AccountantService _accountantService;

        public BaseTest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            _context = new AppDbContext(options);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AccountantService>>();
            _accountantService = new AccountantService(_context, _mockConfiguration.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void ClearDatabase()
        {
            _context.Database.EnsureDeleted(); 
            _context.Database.EnsureCreated(); 
        }

        protected async Task SeedUsersAsync()
        {
            _context.Users.AddRange(
                new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 30,
                    Email = "john.doe@example.com",
                    Salary = 50000,
                    Role = "User",  
                    IsBlocked = false, 
                },
                new User
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Age = 28,
                    Email = "jane.smith@example.com",
                    Salary = 45000,
                    Role = "User",  
                    IsBlocked = false, 
                }
            );
            await _context.SaveChangesAsync();
        }

        protected async Task SeedAccountantsAsync()
        {
            _context.Accountants.AddRange(
                new Accountant
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Role = "Accountant",  
                },
                new Accountant
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Williams",
                    Role = "Accountant", 
                }
            );
            await _context.SaveChangesAsync();
        }

        protected async Task SeedLoansAsync()
        {
            _context.Loans.AddRange(
                new Loan
                {
                    Id = 1,
                    LoanType = (Models.Enums.LoanType)Enums.LoanType.Auto,
                    Amount = 50000,
                    Currency = (Models.Enums.Currency)Enums.Currency.USD,
                    Period = (Models.Enums.Period)Enums.Period.OneMonth,
                    LoanStatus = (Models.Enums.LoanStatus)Enums.LoanStatus.Approved,
                    UserId = 1  
                },
                new Loan
                {
                    Id = 2,
                    LoanType = (Models.Enums.LoanType)Enums.LoanType.Fast,
                    Amount = 20000,
                    Currency = (Models.Enums.Currency)Enums.Currency.USD,
                    Period = (Models.Enums.Period)Enums.Period.SixMonth,
                    LoanStatus = (Models.Enums.LoanStatus)Enums.LoanStatus.Denied,
                    UserId = 2  
                }
            );
            await _context.SaveChangesAsync();
        }

        protected async Task SeedCredentialsAsync()
        {
            _context.Credentials.AddRange(
                new Credentials
                {
                    Id = 1,
                    UserName = "user1",
                    Password = "password1"
                },
                new Credentials
                {
                    Id = 2,
                    UserName = "user2",
                    Password = "password2"
                }
            );
            await _context.SaveChangesAsync();
        }

        protected async Task SeedAccountantCredentialsAsync()
        {
            _context.AccountantCredentials.AddRange(
                new AccountantCredentials
                {
                    Id = 1,
                    UserName = "accountant1",
                    Password = "password1"
                },
                new AccountantCredentials
                {
                    Id = 2,
                    UserName = "accountant2",
                    Password = "password2"
                }
            );
            await _context.SaveChangesAsync();
        }
    }
}



