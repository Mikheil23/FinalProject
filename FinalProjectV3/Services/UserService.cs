using FinalProjectV3.Context;
using FinalProjectV3.DTOS;
using FinalProjectV3.Helpers;
using FinalProjectV3.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static FinalProjectV3.Models.Enums;

namespace FinalProjectV3.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ValidationHelper _validationHelper;
        private readonly ILogger<UserService> _logger;


        public UserService(AppDbContext context, IConfiguration configuration, ValidationHelper validationHelper, ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
            _validationHelper = validationHelper;
            _logger = logger;
            
        }
        public async Task<LoanRequestResponse> AddLoanRequestAsync(LoanRequestDto loanRequestDto, int userId, string role, string loggedInUserId)
        {
            
            _logger.LogInformation("Processing loan request for user {UserId} with loan type {LoanType}", userId, loanRequestDto.LoanType);
            await _validationHelper.ValidateAsync(loanRequestDto);
            if (role != "User")
            {
                _logger.LogWarning("Unauthorized access attempt for user {UserId} with role {Role}.", userId, role);
                throw new UnauthorizedAccessException("User does not have sufficient permissions.");
            }
            if (userId.ToString() != loggedInUserId)
            {
                _logger.LogWarning("User {UserId} attempted to add loan for another account.", userId);
                throw new UnauthorizedAccessException("You can only add loans for your own account.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.IsBlocked)
            {
                _logger.LogWarning("User {UserId} is either invalid or blocked.", userId);
                throw new UnauthorizedAccessException("Invalid user or user is blocked.");
            }

            var loan = new Loan
            {
                UserId = user.Id,
                LoanType = loanRequestDto.LoanType,
                Amount = loanRequestDto.Amount,
                Currency = loanRequestDto.Currency,
                Period = loanRequestDto.Period,
                LoanStatus = LoanStatus.InProgress 
            };

            try
            {
                _logger.LogInformation("Saving loan request for user {UserId} to the database", userId);
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Loan request successfully saved for user {UserId} with Loan ID {LoanId}", userId, loan.Id);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error saving loan request for user {UserId}: {Error}", userId, ex.Message);
                throw new InvalidOperationException("An error occurred while saving the loan request.", ex);
            }
            return new LoanRequestResponse
            {
                LoanId = loan.Id,
                LoanType = loan.LoanType,
                Amount = loan.Amount,
                Currency = loan.Currency,
                Period = loan.Period,
                LoanStatus = loan.LoanStatus
            };
        }
        public async Task<RegistrationResponse> ViewUserCabinetByUserIdAsync(int userId, string loggedInUserId)
        {
            _logger.LogInformation("Validating request to view user cabinet for user {UserId} by logged-in user {LoggedInUserId}.", userId, loggedInUserId);
            if (userId.ToString() != loggedInUserId)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access cabinet of user {LoggedInUserId}.", userId, loggedInUserId);
                throw new UnauthorizedAccessException("You can only view your own cabinet.");
            }

            var user = await _context.Users
                   .Include(u => u.Credentials) 
                   .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID {UserId}.", userId);
                throw new KeyNotFoundException("User not found.");
            }

            var response = new RegistrationResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Age = user.Age,
                Salary = user.Salary,
                Username = user.Credentials?.UserName

            };
            _logger.LogInformation("Successfully retrieved user cabinet for user {UserId}.", userId);
            return response;
        }
        public async Task<IEnumerable<LoanRequestResponse>> ViewAllLoansAsync(int userId, string loggedInUserId)
        {
            _logger.LogInformation("Validating request to view all loans for user {UserId} by logged-in user {LoggedInUserId}.", userId, loggedInUserId);
            if (userId.ToString() != loggedInUserId)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access loans of user {LoggedInUserId}.", userId, loggedInUserId);
                throw new UnauthorizedAccessException("You can only view your own loans.");
            }

            var loans = await _context.Loans
                .Where(loan => loan.UserId == userId)
                .ToListAsync();

            if (!loans.Any())
            {
                _logger.LogWarning("No loans found for user {UserId}.", userId);
                throw new KeyNotFoundException("No loans found for this user.");
            }

            var loanResponses = loans.Select(loan => new LoanRequestResponse
            {
                LoanId = loan.Id,
                LoanType = loan.LoanType,
                Amount = loan.Amount,
                Currency = loan.Currency,
                Period = loan.Period,
                LoanStatus = loan.LoanStatus
            });
            _logger.LogInformation("Successfully retrieved {LoanCount} loans for user {UserId}.", loans.Count, userId);
            return loanResponses;
        }
        public async Task<LoanRequestResponse> UpdateLoanAsync(int loanId, LoanRequestDto loanRequestDto, int userId, string role, string loggedInUserId)
        {
            _logger.LogInformation("Starting update process for loan {LoanId} by user {UserId}.", loanId, userId);
            if (role != "User")
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} with role {Role} tried to update loan {LoanId}.", userId, role, loanId);
                throw new UnauthorizedAccessException("Only users can update loans.");
            }

            if (userId.ToString() != loggedInUserId)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to update loan {LoanId}, but logged-in user is {LoggedInUserId}.", userId, loanId, loggedInUserId);
                throw new UnauthorizedAccessException("You can only update your own loans.");
            }
            await _validationHelper.ValidateAsync(loanRequestDto);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.IsBlocked)
            {
                _logger.LogWarning("Invalid or blocked user {UserId} attempted to update loan {LoanId}.", userId, loanId);
                throw new UnauthorizedAccessException("Invalid user or user is blocked.");
            }

            var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);

            if (loan == null)
            {
                _logger.LogWarning("Loan {LoanId} not found for user {UserId}.", loanId, userId);
                throw new KeyNotFoundException("Loan not found.");
            }

            if (loan.LoanStatus != LoanStatus.InProgress)
            {
                _logger.LogWarning("Loan {LoanId} status is {LoanStatus}, update is not allowed for non-in-progress loans.", loanId, loan.LoanStatus);
                throw new InvalidOperationException("You can only update loans that are in progress.");
            }

            loan.LoanType = loanRequestDto.LoanType;
            loan.Amount = loanRequestDto.Amount;
            loan.Currency = loanRequestDto.Currency;
            loan.Period = loanRequestDto.Period;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Loan {LoanId} successfully updated by user {UserId}.", loanId, userId);
            return new LoanRequestResponse
            {
                LoanId = loan.Id,
                LoanType = loan.LoanType,
                Amount = loan.Amount,
                Currency = loan.Currency,
                Period = loan.Period,
                LoanStatus = loan.LoanStatus
            };
        }
        public async Task<bool> DeleteLoanAsync(int userId, int loanId, string role, string loggedInUserId)
        {
            _logger.LogInformation("User {UserId} is attempting to delete loan {LoanId}.", userId, loanId);
            if (role != "User")
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} with role {Role} tried to delete loan {LoanId}.", userId, role, loanId);
                throw new UnauthorizedAccessException("Only users can delete loans.");
            }

            if (userId.ToString() != loggedInUserId)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to delete loan {LoanId}, but logged-in user is {LoggedInUserId}.", userId, loanId, loggedInUserId);
                throw new UnauthorizedAccessException("You can only delete your own loans.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.IsBlocked)
            {
                _logger.LogWarning("Invalid or blocked user {UserId} attempted to delete loan {LoanId}.", userId, loanId);
                throw new UnauthorizedAccessException("Invalid user or user is blocked.");
            }

            var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);

            if (loan == null)
            {
                _logger.LogWarning("Loan {LoanId} not found for user {UserId}.", loanId, userId);
                throw new KeyNotFoundException("Loan not found.");
            }

            if (loan.LoanStatus != LoanStatus.InProgress)
            {
                _logger.LogWarning("Loan {LoanId} status is {LoanStatus}, deletion is not allowed for non-in-progress loans.", loanId, loan.LoanStatus);
                throw new InvalidOperationException("You can only delete loans that are in progress.");
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Loan {LoanId} successfully deleted by user {UserId}.", loanId, userId);
            return true; 
        }
    }
}
