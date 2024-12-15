using FinalProjectV3.Context;
using FinalProjectV3.Models;
using FinalProjectV3.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;
using static FinalProjectV3.Models.Enums;

namespace FinalProjectV3.Tests
{
    public class AccountantServiceTests : BaseTest
    {
        private readonly AccountantService _accountantService;

        public AccountantServiceTests()
        {
            var mockLogger = new Mock<ILogger<AccountantService>>();
            _accountantService = new AccountantService(_context, _mockConfiguration.Object, mockLogger.Object);
        }
        [Fact]
        public async Task ViewAllUsersAsync_ShouldReturnAllUsers()
        {
            await SeedUsersAsync();

            var result = await _accountantService.ViewAllUsersAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0].FirstName);
            Assert.Equal("Doe", result[0].LastName);
            Assert.Equal("Jane", result[1].FirstName);
        }
        [Fact]
        public async Task BlockOrUnblockUserAsync_ShouldThrowException_WhenUserIsAlreadyBlockedOrUnblocked()
        {
            await SeedUsersAsync();
            var user = await _context.Users.FindAsync(1);
            user.IsBlocked = true; 
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _accountantService.BlockOrUnblockUserAsync(user.Id, true));

            Assert.Equal("User is already blocked.", exception.Message);
        }



        [Fact]
        public async Task ViewAllUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            _context.Users.RemoveRange(_context.Users); 
            await _context.SaveChangesAsync();

            var result = await _accountantService.ViewAllUsersAsync();

            Assert.NotNull(result); 
            Assert.Empty(result);  
        }

        






    [Fact]
        public async Task GetAllLoanRequestsAsync_ShouldReturnEmptyList_WhenNoLoansExist()
        {
            _context.Loans.RemoveRange(_context.Loans); 
            await _context.SaveChangesAsync();

            var result = await _accountantService.GetAllLoanRequestsAsync();

            Assert.NotNull(result);  
            Assert.Empty(result);   
        }
        [Fact]
        public async Task GetAllLoanRequestsAsync_ShouldHandleLoansWithNegativeAmounts()
        {
            var loanWithNegativeAmount = new Loan
            {
                Amount = -1000,
                Currency = Currency.USD,
                LoanType = LoanType.Auto,
                LoanStatus = LoanStatus.InProgress,
                UserId = 1
            };
            _context.Loans.Add(loanWithNegativeAmount);
            await _context.SaveChangesAsync();

            var result = await _accountantService.GetAllLoanRequestsAsync();

            Assert.Single(result);
            Assert.Equal(-1000, result[0].Amount); 
        }

        [Fact]
        public async Task BlockOrUnblockUserAsync_ShouldBlockUser_WhenUserIsNotBlocked()
        {
            await SeedUsersAsync();

            var result = await _accountantService.BlockOrUnblockUserAsync(1, true);

            Assert.True(result); 
            var user = await _context.Users.FindAsync(1);
            Assert.True(user.IsBlocked); 
        }
        [Fact]
        public async Task BlockOrUnblockUserAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _accountantService.BlockOrUnblockUserAsync(999, true));
        }
        [Fact]
        public async Task BlockOrUnblockUserAsync_ShouldHandleConcurrentRequests()
        {
            await SeedUsersAsync();
            var userId = 1;

            var tasks = new[]
            {
        _accountantService.BlockOrUnblockUserAsync(userId, true),
        _accountantService.BlockOrUnblockUserAsync(userId, false)
    };
            await Task.WhenAll(tasks);

            var user = await _context.Users.FindAsync(userId);
            Assert.False(user.IsBlocked); 
        }

        [Fact]
        public async Task ChangeLoanStatusAsync_ShouldSuccessfullyChangeStatus_WhenConditionsAreMet()
        {
            await SeedUsersAsync();
            await SeedLoansAsync();

            var userId = 1;
            var loanId = 1;
            var newStatus = Models.Enums.LoanStatus.Denied; 

            var result = await _accountantService.ChangeLoanStatusAsync(userId, loanId, newStatus);

            var loan = await _context.Loans.FindAsync(loanId);
            Assert.NotNull(loan); 
            Assert.Equal(newStatus, loan.LoanStatus); 
            Assert.True(result); 
        }
        

        [Fact]
        public async Task ChangeLoanStatusAsync_ShouldThrowException_WhenUserIsBlocked()
        {
            await SeedUsersAsync();
            await SeedLoansAsync();

            var user = await _context.Users.FindAsync(1);
            user.IsBlocked = true;
            await _context.SaveChangesAsync();

            var loanId = 1;
            var newStatus = Models.Enums.LoanStatus.Approved;

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _accountantService.ChangeLoanStatusAsync(user.Id, loanId, newStatus));

            Assert.Equal("User is blocked.", exception.Message); 
        }
        [Fact]
        public async Task ChangeLoanStatusAsync_ShouldThrowException_WhenLoanNotFound()
        {
            await SeedUsersAsync();

            var userId = 1;
            var loanId = 9999; 
            var newStatus = Models.Enums.LoanStatus.Approved;

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _accountantService.ChangeLoanStatusAsync(userId, loanId, newStatus));

            Assert.Equal("Loan not found for the specified user.", exception.Message); 
        }
        [Fact]
        public async Task ChangeLoanStatusAsync_ShouldThrowException_WhenLoanAlreadyHasNewStatus()
        {
            await SeedUsersAsync();
            await SeedLoansAsync();

            var userId = 1;
            var loanId = 1;
            var newStatus = Models.Enums.LoanStatus.Approved; 

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _accountantService.ChangeLoanStatusAsync(userId, loanId, newStatus));

            Assert.Equal("Loan is already approved.", exception.Message); 
        }
        [Fact]
        public async Task DeleteLoanAsync_ShouldDeleteLoan_WhenLoanExists()
        {
            await SeedUsersAsync();
            await SeedLoansAsync(); 

            var loanId = 1; 
            var service = new AccountantService(_context, _mockConfiguration.Object, _mockLogger.Object);

            var result = await service.DeleteLoanAsync(loanId);

            var deletedLoan = await _context.Loans.FindAsync(loanId);
            Assert.Null(deletedLoan); 
            Assert.True(result); 
        }
        [Fact]
        public async Task DeleteLoanAsync_ShouldThrowException_WhenLoanDoesNotExist()
        {
            await SeedUsersAsync();
            await SeedLoansAsync(); 

            var nonExistentLoanId = 999; 
            var service = new AccountantService(_context, _mockConfiguration.Object, _mockLogger.Object);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.DeleteLoanAsync(nonExistentLoanId));

            Assert.Equal("Loan not found.", exception.Message); 
        }
        


        [Fact]
        public async Task ViewAllUsersAsync_ShouldHandleUsersWithoutCredentials()
        {
            var userWithoutCredentials = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@example.com",
                Age = 30,
                Salary = 50000
            };
            _context.Users.Add(userWithoutCredentials);
            await _context.SaveChangesAsync();

            var result = await _accountantService.ViewAllUsersAsync();

            Assert.Single(result);
            Assert.Null(result[0].Username); 
        }
        [Fact]
        public async Task DeleteLoanAsync_ShouldHandleLoanWithZeroAmount()
        {
            var loanWithZeroAmount = new Loan
            {
                Amount = 0,
                Currency = Currency.USD,
                LoanType = LoanType.Fast,
                LoanStatus = LoanStatus.InProgress,
                UserId = 1
            };
            _context.Loans.Add(loanWithZeroAmount);
            await _context.SaveChangesAsync();

            var result = await _accountantService.DeleteLoanAsync(loanWithZeroAmount.Id);

            Assert.True(result);
            Assert.Null(await _context.Loans.FindAsync(loanWithZeroAmount.Id)); 
        }









    }
}



