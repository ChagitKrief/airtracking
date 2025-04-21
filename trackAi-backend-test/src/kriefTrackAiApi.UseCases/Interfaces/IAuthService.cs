using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Common.Dto;
using System.Threading.Tasks;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure.Email;

namespace kriefTrackAiApi.UseCases.Interfaces;

public interface IAuthService
{
    Task SendTemporaryPasswordAsync(Guid userId);
    Task<bool> VerifyTemporaryPasswordAsync(Guid userId, string providedPassword);
    Task ResetPasswordAsync(Guid userId, string tempPassword, string newPassword);
    Task<LoginResponseDto> GenerateLoginResponse(User user, bool generateNewToken = false);
    Task SendTemporaryPasswordByEmailAsync(string email);
    Task<bool> VerifyTemporaryPasswordByEmailAsync(string email, string providedPassword);
    Task ResetPasswordByEmailAsync(string email, string tempPassword, string newPassword);

}
