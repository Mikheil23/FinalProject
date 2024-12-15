using FinalProjectV3.DTOS;
using FinalProjectV3.Helpers;
using FinalProjectV3.Models;
using FinalProjectV3.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProjectV3.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ValidationHelper _validationHelper;
        private string loggedInUserId;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ValidationHelper validationHelper, ILogger<UserController> logger)
        {
            _userService = userService;
            _validationHelper = validationHelper;
            _logger = logger;
        }

        [HttpPost("loan-request")]
        
        public async Task<IActionResult> AddLoanRequest([FromBody] LoanRequestDto loanRequestDto)
        {
            try
            {
                _logger.LogInformation("Loan request received from user {UserId} for loan type {LoanType}",
               User.FindFirstValue(ClaimTypes.NameIdentifier), loanRequestDto.LoanType);
                await _validationHelper.ValidateAsync(loanRequestDto);

                var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var role = User.FindFirstValue(ClaimTypes.Role);
                _logger.LogInformation("Validating loan request for user {UserId}, role {Role}", loggedInUserId, role);
                var loanRequestResponse = await _userService.AddLoanRequestAsync(loanRequestDto, int.Parse(loggedInUserId), role, loggedInUserId);
                _logger.LogInformation("Loan request successfully processed for user {UserId}", loggedInUserId);
                return Ok(loanRequestResponse);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for loan request: {Error}", ex.Message);
                return BadRequest(new { message = "Validation failed.", errors = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt: {Error}", ex.Message);
                return StatusCode(403, new { message = "Forbidden", details = ex.Message });
            }

            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {Error}", ex.Message);
                return StatusCode(500, new { message = "An error occurred during login.", details = ex.Message });
            }

        }
        
        [HttpGet("user-cabinet/{userId}")]
        public async Task<IActionResult> ViewUserCabinet(int userId)
        {
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

          
            _logger.LogInformation("Attempt to access user cabinet. Logged in User ID: {LoggedInUserId}", loggedInUserId);
            if (string.IsNullOrEmpty(loggedInUserId))
            {
                _logger.LogWarning("Unauthorized attempt: No valid token found for accessing the user cabinet.");
                return Unauthorized("Invalid token.");
            }

            try
            {
                if (userId.ToString() != loggedInUserId)
                {
                    _logger.LogWarning("Unauthorized access attempt by user {UserId} to view cabinet of user {LoggedInUserId}.", userId, loggedInUserId);
                    throw new UnauthorizedAccessException("You can only view your own cabinet.");
                }

                var userCabinet = await _userService.ViewUserCabinetByUserIdAsync(userId, loggedInUserId);
                _logger.LogInformation("Successfully retrieved user cabinet for user {UserId}.", userId);
                return Ok(userCabinet);  
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt: {Error}", ex.Message);
                return Forbid();  

            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("User not found: {Error}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred while accessing the user cabinet: {Error}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while accessing the user cabinet.", details = ex.Message });
            }
        }
        [HttpGet("{userId}/view-loans-history")]
        public async Task<IActionResult> ViewAllLoans(int userId)
        {
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} is attempting to view loans for user {LoggedInUserId}.", userId, loggedInUserId);

            if (string.IsNullOrEmpty(loggedInUserId) || loggedInUserId != userId.ToString())
            {
                _logger.LogWarning("Unauthorized access attempt: Token is invalid or user ID mismatched for user {UserId}.", userId);
                return Unauthorized("Invalid token or mismatched user ID.");
            }

            try
            {
                var loans = await _userService.ViewAllLoansAsync(userId, loggedInUserId);
                _logger.LogInformation("Successfully retrieved {LoanCount} loans for user {UserId}.", loans.Count(), userId);
                return Ok(loans); 
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt: {Error}", ex.Message);
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("No loans found for user {UserId}: {Error}", userId, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred while retrieving loans for user {UserId}: {Error}", userId, ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving loans.", details = ex.Message });
            }
        }
        [HttpPut("users/{userId}/loans/{loanId}/update-loan")]
        public async Task<IActionResult> UpdateLoan(int userId, int loanId, [FromBody] LoanRequestDto loanRequestDto)
        {
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} is attempting to update loan {LoanId}. Logged-in user ID: {LoggedInUserId}.", userId, loanId, loggedInUserId);
            if (string.IsNullOrEmpty(loggedInUserId))
            {
                _logger.LogWarning("User {UserId} attempted to update loan {LoanId}, but the token is invalid or missing.", userId, loanId);
                return Unauthorized(new { message = "Unauthorized" });
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                var updatedLoan = await _userService.UpdateLoanAsync(loanId, loanRequestDto, userId, role, loggedInUserId);
                _logger.LogInformation("Loan {LoanId} successfully updated by user {UserId}.", loanId, userId);
                return Ok(updatedLoan);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for loan update. User {UserId}: {Error}", userId, ex.Message);
                return BadRequest(new { message = "Validation failed.", errors = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt to update loan {LoanId} by user {UserId}: {Error}", loanId, userId, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation while updating loan {LoanId} for user {UserId}: {Error}", loanId, userId, ex.Message);
                return Conflict(new { message = ex.Message }); 
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Loan not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });  
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred while updating loan {LoanId} for user {UserId}: {Error}", loanId, userId, ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating the loan.", details = ex.Message });
            }
           
        }
        [HttpDelete("users/{userId}/loans/{loanId}/delete-loan")]
        public async Task<IActionResult> DeleteLoan(int userId, int loanId)
        {
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} is attempting to delete loan {LoanId}. Logged-in user ID: {LoggedInUserId}.", userId, loanId, loggedInUserId);
            if (string.IsNullOrEmpty(loggedInUserId))
            {
                _logger.LogWarning("User {UserId} attempted to delete loan {LoanId}, but the token is invalid or missing.", userId, loanId);
                return Unauthorized("Invalid token.");
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                var result = await _userService.DeleteLoanAsync(userId, loanId, role, loggedInUserId);

                if (result)
                {
                    _logger.LogInformation("Loan {LoanId} successfully deleted by user {UserId}.", loanId, userId);
                    return Ok(new { message = "Loan has been successfully deleted." }); 
                }
                _logger.LogWarning("Failed to delete loan {LoanId} for user {UserId}.", loanId, userId);
                return BadRequest("Failed to delete the loan.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt to delete loan {LoanId} by user {UserId}: {Error}", loanId, userId, ex.Message);
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Loan {LoanId} not found for user {UserId}: {Error}", loanId, userId, ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation while deleting loan {LoanId} for user {UserId}: {Error}", loanId, userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred while deleting loan {LoanId} for user {UserId}: {Error}", loanId, userId, ex.Message);
                return StatusCode(500, new { message = "An error occurred while deleting the loan.", details = ex.Message });
            }
        }





    }
}
