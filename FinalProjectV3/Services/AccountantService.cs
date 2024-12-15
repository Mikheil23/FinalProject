using FinalProjectV3.Context;
using FinalProjectV3.DTOS;
using FinalProjectV3.Models;
using Microsoft.EntityFrameworkCore;
using static FinalProjectV3.Models.Enums;

namespace FinalProjectV3.Services
{
    public class AccountantService : IAccountantService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountantService> _logger;


       
        public AccountantService(AppDbContext context, IConfiguration configuration, ILogger<AccountantService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<List<RegistrationResponse>> ViewAllUsersAsync()
        {
            try
            {

                _logger.LogInformation("Fetching all users from the database.");
               
                var users = await _context.Users
                    .Include(u => u.Credentials) 
                    .ToListAsync();


               
                var userResponses = users.Select(user => new RegistrationResponse
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Age = user.Age,
                    Salary = user.Salary,
                    Username = user.Credentials?.UserName
                }).ToList();
                _logger.LogInformation($"Successfully fetched {userResponses.Count} users.");
                return userResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users from the database.");
                throw; 
            }

        }
        public async Task<List<LoanRequestResponse>> GetAllLoanRequestsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all loan requests from the database.");
                
                var loans = await _context.Loans.ToListAsync();

               
                var loanResponses = loans.Select(loan => new LoanRequestResponse
                {
                    LoanId = loan.Id,
                    LoanType = loan.LoanType,
                    Amount = loan.Amount,
                    Currency = loan.Currency,
                    Period = loan.Period,
                    LoanStatus = loan.LoanStatus
                }).ToList();
                _logger.LogInformation($"Successfully fetched {loanResponses.Count} loan requests.");
                return loanResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching loan requests.");
                throw;
            }
        }
        public async Task<bool> BlockOrUnblockUserAsync(int userId, bool isBlocked)
        {
            try
            {
                _logger.LogInformation("Attempting to {Action} user with ID {UserId}.",
                                        isBlocked ? "block" : "unblock", userId);
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

               
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    throw new KeyNotFoundException("User not found.");
                }

                
                if (user.IsBlocked == isBlocked)
                {

                    var status = isBlocked ? "blocked" : "unblocked";
                    _logger.LogWarning("User with ID {UserId} is already {Status}.", userId, status);
                    throw new InvalidOperationException($"User is already {status}.");
                }

               
                user.IsBlocked = isBlocked;
               

                
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID {UserId} successfully {Action}.", userId,
                                    isBlocked ? "blocked" : "unblocked");

                return true; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while attempting to block/unblock user with ID {UserId}.", userId);
                throw;

            }
        }
        public async Task<bool> ChangeLoanStatusAsync(int userId, int loanId, LoanStatus newStatus)
        {
            try
            {
                _logger.LogInformation("Attempting to change loan status for user with ID {UserId} and loan with ID {LoanId} to {NewStatus}.", userId, loanId, newStatus);
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

               
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    throw new KeyNotFoundException("User not found.");
                }

                if (user.IsBlocked)
                {
                    _logger.LogWarning("User with ID {UserId} is blocked.", userId);
                    throw new UnauthorizedAccessException("User is blocked.");
                }

                
                var loan = await _context.Loans
                    .FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);

                if (loan == null)
                {
                    _logger.LogWarning("Loan with ID {LoanId} not found for user with ID {UserId}.", loanId, userId);
                    throw new KeyNotFoundException("Loan not found for the specified user.");
                }

              
                if (loan.LoanStatus == newStatus)
                {
                    _logger.LogWarning("Loan with ID {LoanId} already has status {Status}.", loanId, newStatus);
                    var status = newStatus.ToString().ToLower();
                    throw new InvalidOperationException($"Loan is already {status}.");
                }

                
                loan.LoanStatus = newStatus;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully changed loan status for loan with ID {LoanId} to {NewStatus}.", loanId, newStatus);

                return true; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing loan status for loan with ID {LoanId} and user with ID {UserId}.", loanId, userId);
                throw;
            }
        }
        public async Task<bool> DeleteLoanAsync(int loanId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete loan with ID {LoanId}.", loanId);


                var loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == loanId);

                if (loan == null)
                {
                    _logger.LogWarning("Loan with ID {LoanId} not found.", loanId);
                    throw new KeyNotFoundException("Loan not found.");
                }

                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Loan with ID {LoanId} successfully deleted.", loanId);
                return true; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting loan with ID {LoanId}.", loanId);
                throw; 
            }
        }



    }
}
