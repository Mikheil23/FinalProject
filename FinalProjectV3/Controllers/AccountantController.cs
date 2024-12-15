using FinalProjectV3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using static FinalProjectV3.Models.Enums;

namespace FinalProjectV3.Controllers
{
    [Authorize(Roles = "Accountant")]
    [ApiController]
    [Route("api/[controller]")]
    
    public class AccountantController : ControllerBase
    {
        
        private readonly IAccountantService _accountantService;
        private readonly ILogger<AccountantController> _logger;

        public AccountantController(IAccountantService accountantService, ILogger<AccountantController> logger)
        {
            _accountantService = accountantService;
            _logger = logger;
        }

        [HttpGet("view-users")]
        public async Task<IActionResult> ViewAllUsers()
        {
            try
            {

                _logger.LogInformation("Fetching all users.");
                var users = await _accountantService.ViewAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                    _logger.LogError(ex, "Error occurred while fetching users.");
                    return StatusCode(500, new { Error = "An error occurred while fetching users." });
            }
            
            
        }
        [HttpGet("view-loan-requests")]
        public async Task<IActionResult> GetAllLoanRequests()
        {
            try
            {
                _logger.LogInformation("Fetching loan requests from the service.");
                var loans = await _accountantService.GetAllLoanRequestsAsync();
                _logger.LogInformation($"Successfully retrieved {loans.Count} loan requests.");
                return Ok(loans);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error occurred while fetching loan requests.");
                return StatusCode(500, new { Error = "An error occurred while fetching loan requests." });
            }
        }
        [HttpPatch("block-or-unblock-user/{userId}")]
        public async Task<IActionResult> BlockOrUnblockUser(int userId, [FromQuery] bool isBlocked)
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Error = "Unauthorized" });
            }
            try
            {
                _logger.LogInformation("Received request to {Action} user with ID {UserId}.",
                                isBlocked ? "block" : "unblock", userId);
                await _accountantService.BlockOrUnblockUserAsync(userId, isBlocked);
                var action = isBlocked ? "blocked" : "unblocked";
                _logger.LogInformation("User with ID {UserId} successfully {Action}.", userId, action);
                return Ok(new { Message = $"User successfully {action}." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("User with ID {UserId} not found. Error: {Error}", userId, ex.Message);
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation for user with ID {UserId}. Error: {Error}", userId, ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing request to block/unblock user with ID {UserId}.", userId);
                return StatusCode(500, new { Error = "An error occurred.", Details = ex.Message });
            }
        }
        [HttpPatch("change-loan-status/{userId}/{loanId}")]
        public async Task<IActionResult> ChangeLoanStatus(int userId, int loanId, [FromQuery] LoanStatus newStatus)
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Error = "Unauthorized" });
            }

            try
            {
                _logger.LogInformation("Received request to change loan status for user with ID {UserId} and loan with ID {LoanId} to {NewStatus}.", userId, loanId, newStatus);

                
                bool success = await _accountantService.ChangeLoanStatusAsync(userId, loanId, newStatus);

                if (success)
                {
                    _logger.LogInformation("Successfully changed loan status for loan with ID {LoanId} to {NewStatus}.", loanId, newStatus);
                    return Ok(new { Message = $"Loan status successfully changed to {newStatus}." });
                }
                _logger.LogWarning("Failed to change loan status for loan with ID {LoanId}.", loanId);

                return BadRequest(new { Error = "Failed to change loan status." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Error}", ex.Message);
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access: {Error}", ex.Message);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while processing the request.");
                return StatusCode(500, new { Error = "An internal error occurred. Please contact support if the issue persists." });
            }
        }
        [HttpDelete("delete-loan/{loanId}")]
        public async Task<IActionResult> DeleteLoan(int loanId)
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Error = "Unauthorized" });
            }
            try
            {
                _logger.LogInformation("Received request to delete loan with ID {LoanId}.", loanId);
               
                bool success = await _accountantService.DeleteLoanAsync(loanId);

                if (success)
                {
                    _logger.LogInformation("Loan with ID {LoanId} successfully deleted.", loanId);
                    return Ok(new { Message = "Loan successfully deleted." });
                }
                _logger.LogWarning("Failed to delete loan with ID {LoanId}.", loanId);
                return BadRequest(new { Error = "Failed to delete loan." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Loan with ID {LoanId} not found. Error: {Error}", loanId, ex.Message);
                return NotFound(new { Error = "Loan not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting loan with ID {LoanId}.", loanId);
                return StatusCode(500, new { Error = "An error occurred.", Details = ex.Message });
            }
        }
    }
}

