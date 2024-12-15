using FinalProjectV3.DTOS;
using FinalProjectV3.Helpers;
using FinalProjectV3.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectV3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ValidationHelper _validationHelper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ValidationHelper validationHelper, ILogger<AuthController> logger)
        {
            _authService = authService;
            _validationHelper = validationHelper;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<ActionResult<RegistrationResponse>> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                _logger.LogWarning("Received null registration request.");
                return BadRequest("Invalid request data.");
            }
            _logger.LogInformation("Received registration request for user with email: {Email}", registerDto.Email);
            try
            {
                var response = await _authService.RegisterAsync(registerDto);
                _logger.LogInformation("User successfully registered with email: {Email}", registerDto.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("register-accountant")]
        public async Task<ActionResult<AccountantRegistrationResponse>> RegisterAccountantAsync([FromBody] AccountantRegisterDto accountantRegisterDto)
        {
            if (accountantRegisterDto == null)
            {
                _logger.LogWarning("Received null registration request for accountant.");
                return BadRequest("Invalid request data.");
            }
            _logger.LogInformation("Received accountant registration request for username: {UserName}", accountantRegisterDto.UserName);
            try
            {
                var response = await _authService.RegisterAccountantAsync(accountantRegisterDto);
                _logger.LogInformation("Accountant successfully registered with username: {UserName}", accountantRegisterDto.UserName);
                return Ok(response); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during accountant registration for username: {UserName}.", accountantRegisterDto.UserName);
                return BadRequest(ex.Message); 
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt started for username: {Username}", loginDto.Username);
            try
            {
                await _validationHelper.ValidateAsync(loginDto);

                var loginResponse = await _authService.LoginAsync(loginDto);
                _logger.LogInformation("Login successful for username: {Username}.", loginDto.Username);
                return Ok(loginResponse); 
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for login attempt for username: {Username}. Error: {Error}", loginDto.Username, ex.Message);
                return BadRequest(new { message = "Validation failed.", errors = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access for username: {Username}. Error: {Error}", loginDto.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for username: {Username}.", loginDto.Username);
                return StatusCode(500, new { message = "An error occurred during login.", details = ex.Message });
            }
        }
    }


}
