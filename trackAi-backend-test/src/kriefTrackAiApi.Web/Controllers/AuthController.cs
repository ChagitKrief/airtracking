using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Common.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using kriefTrackAiApi.UseCases.Interfaces;



[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public AuthController(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        var user = await _userRepository.ValidateUserAsync(loginDto.Email, loginDto.Password);
        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var response = await _authService.GenerateLoginResponse(user, generateNewToken: true);

        Console.WriteLine("Response Sent to Client: " + System.Text.Json.JsonSerializer.Serialize(response));

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized("Invalid token");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var response = await _authService.GenerateLoginResponse(user, generateNewToken: false);
        return Ok(response);
    }

    [HttpPost("reset-password-request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] Guid userId)
    {
        try
        {
            await _authService.SendTemporaryPasswordAsync(userId);
            return Ok("Temporary password sent.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("verify-temp-password")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTempPassword([FromBody] TempPasswordVerificationRequest dto)
    {
        var isValid = await _authService.VerifyTemporaryPasswordAsync(dto.UserId, dto.TempPassword);
        return isValid ? Ok("Valid") : Unauthorized("Invalid or expired password.");
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest dto)
    {
        try
        {
            await _authService.ResetPasswordAsync(dto.UserId, dto.TempPassword, dto.NewPassword);
            return Ok("Password reset successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("reset-password-request-by-email")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordResetByEmail([FromBody] string email)
    {
        try
        {
            await _authService.SendTemporaryPasswordByEmailAsync(email);
            return Ok("Temporary password sent.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("verify-temp-password-by-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTempPasswordByEmail([FromBody] TempPasswordEmailVerificationRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.TempPassword))
        {
            return BadRequest("Missing email or temporary password.");
        }

        var isValid = await _authService.VerifyTemporaryPasswordByEmailAsync(dto.Email, dto.TempPassword);
        return isValid ? Ok("Valid") : Unauthorized("Invalid or expired password.");
    }

    [HttpPost("reset-password-by-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPasswordByEmail([FromBody] PasswordResetByEmailRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.TempPassword) ||
            string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest("Missing email, temporary password or new password.");
        }

        try
        {
            await _authService.ResetPasswordByEmailAsync(dto.Email, dto.TempPassword, dto.NewPassword);
            return Ok("Password reset successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


}
