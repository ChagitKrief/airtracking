

using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Infrastructure.Services;
using kriefTrackAiApi.Infrastructure.Email;
using kriefTrackAiApi.UseCases.Interfaces;

namespace kriefTrackAiApi.UseCases.Services;

public class AuthService : IAuthService
{
    private readonly string _jwtSecret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryInMinutes;
    private readonly ICompanyRepository _companyRepository;
    private readonly WinwordQueryService _winwordQueryService;
    private readonly IUserRepository _userRepository;
    private readonly PasswordResetEmailService _passwordResetEmailService;

    public AuthService(
        string jwtSecret,
        string issuer,
        string audience,
        int expiryInMinutes,
        ICompanyRepository companyRepository,
        WinwordQueryService winwordQueryService,
        IUserRepository userRepository,
        PasswordResetEmailService passwordResetEmailService)
    {
        _jwtSecret = jwtSecret;
        _issuer = issuer;
        _audience = audience;
        _expiryInMinutes = expiryInMinutes;
        _companyRepository = companyRepository;
        _winwordQueryService = winwordQueryService;
        _userRepository = userRepository;
        _passwordResetEmailService = passwordResetEmailService;
    }
    private (string Token, DateTime Expiration) GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var expiration = DateTime.UtcNow.AddMinutes(_expiryInMinutes);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
    public async Task<LoginResponseDto> GenerateLoginResponse(User user, bool generateNewToken = false)
    {
        string? token = null;
        DateTime expiration = DateTime.UtcNow;

        if (generateNewToken)
        {
            (token, expiration) = GenerateJwtToken(user);
        }

        // await AddAllCompaniesIfAdminOrManagerAsync(user);

        List<CustomerDto> activeCustomers;
        int[] customerNumbers;

        if (user.Role == 1 || user.Role == 3)
        {
            var companies = await _companyRepository.GetCompaniesByIdsAsync(user.CompanyIds);
            activeCustomers = companies.Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerNumber = c.CustomerNumber,
                CustomerName = c.CustomerName
            }).ToList();
        }
        else
        {
            activeCustomers = await _companyRepository.GetActiveCustomersByIdsAsync(user.CompanyIds);
        }

        if (user.Role == 1 || user.Role == 3)
        {
            var allCompanies = await _companyRepository.GetAllAsync();
            customerNumbers = allCompanies.Select(c => c.CustomerNumber).ToArray();
        }
        else
        {
            customerNumbers = activeCustomers.Select(c => c.CustomerNumber).ToArray();
        }

        List<DataItem> winwordData = new List<DataItem>();
        if (customerNumbers.Any())
        {
            winwordData = await _winwordQueryService.FetchAndFilterShipmentDataAsync(customerNumbers);
        }

        return new LoginResponseDto
        {
            Token = token ?? string.Empty,
            RefreshToken = generateNewToken ? GenerateRefreshToken() : string.Empty,
            Email = user.Email,
            Role = user.Role,
            Expiration = expiration,
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Reminders = user.Reminders ?? Array.Empty<string>(),
            ActiveCustomers = activeCustomers,
            WinwordData = winwordData
        };
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }


    private async Task AddAllCompaniesIfAdminOrManagerAsync(User user)
    {
        if (user.Role == 1 || user.Role == 3)
        {
            var allCompanies = await _companyRepository.GetAllAsync();
            var userCompanyIds = user.CompanyIds?.ToList() ?? new List<Guid>();

            foreach (var company in allCompanies)
            {
                if (!userCompanyIds.Contains(company.Id))
                {
                    userCompanyIds.Add(company.Id);
                }
            }

            user.CompanyIds = userCompanyIds;
            await _userRepository.UpdateAsync(user.Id, user);
        }
    }

    //logic of update password
    private string GenerateTemporaryPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task SendTemporaryPasswordAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var password = GenerateTemporaryPassword(8);
        var expiration = DateTime.UtcNow.AddMinutes(10);

        user.VerificationTmpPass = $"{password}::{expiration.Ticks}";
        await _userRepository.UpdateAsync(user.Id, user);

        await _passwordResetEmailService.SendPasswordResetEmailAsync(user, password);
    }


    public async Task<bool> VerifyTemporaryPasswordAsync(Guid userId, string providedPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.VerificationTmpPass))
            return false;

        var parts = user.VerificationTmpPass.Split("::");
        if (parts.Length != 2) return false;

        var storedPassword = parts[0];
        if (!long.TryParse(parts[1], out long ticks)) return false;

        var expiration = new DateTime(ticks, DateTimeKind.Utc);
        if (DateTime.UtcNow > expiration) return false;

        return storedPassword == providedPassword;
    }

    public async Task ResetPasswordAsync(Guid userId, string tempPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.VerificationTmpPass))
            throw new Exception("User not found or no temporary password set");

        var parts = user.VerificationTmpPass.Split("::");
        if (parts.Length != 2 || parts[0] != tempPassword)
            throw new Exception("Temporary password is incorrect");

        if (!long.TryParse(parts[1], out long ticks) || DateTime.UtcNow > new DateTime(ticks, DateTimeKind.Utc))
            throw new Exception("Temporary password expired");

        user.Password = newPassword;
        user.VerificationTmpPass = string.Empty;
        await _userRepository.UpdateAsync(user.Id, user);
    }
    public async Task SendTemporaryPasswordByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found");

        var password = GenerateTemporaryPassword(8);
        var expiration = DateTime.UtcNow.AddMinutes(10);

        user.VerificationTmpPass = $"{password}::{expiration.Ticks}";
        await _userRepository.UpdateAsync(user.Id, user);

        await _passwordResetEmailService.SendPasswordResetEmailAsync(user, password);
    }
    public async Task<bool> VerifyTemporaryPasswordByEmailAsync(string email, string providedPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || string.IsNullOrEmpty(user.VerificationTmpPass))
            return false;

        var parts = user.VerificationTmpPass.Split("::");
        if (parts.Length != 2) return false;

        var storedPassword = parts[0];
        if (!long.TryParse(parts[1], out long ticks)) return false;

        var expiration = new DateTime(ticks, DateTimeKind.Utc);
        if (DateTime.UtcNow > expiration) return false;

        return storedPassword == providedPassword;
    }

    public async Task ResetPasswordByEmailAsync(string email, string tempPassword, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || string.IsNullOrEmpty(user.VerificationTmpPass))
            throw new Exception("User not found or no temporary password set");

        var parts = user.VerificationTmpPass.Split("::");
        if (parts.Length != 2 || parts[0] != tempPassword)
            throw new Exception("Temporary password is incorrect");

        if (!long.TryParse(parts[1], out long ticks) || DateTime.UtcNow > new DateTime(ticks, DateTimeKind.Utc))
            throw new Exception("Temporary password expired");

        user.Password = newPassword;
        user.VerificationTmpPass = string.Empty;
        await _userRepository.UpdateAsync(user.Id, user);
    }
}
