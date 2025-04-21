using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using kriefTrackAiApi.Core.Models;

namespace kriefTrackAiApi.Infrastructure.Email;

public class PasswordResetEmailService
{
    private readonly EmailService _emailService;

    public PasswordResetEmailService(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendPasswordResetEmailAsync(User user, string tempPassword)
    {
        string htmlTemplatePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "email", "PasswordResetTemplate.html");

        string htmlTemplate = File.Exists(htmlTemplatePath)
            ? await File.ReadAllTextAsync(htmlTemplatePath)
            : "<h1>Template not found</h1>";


        string html = htmlTemplate
            .Replace("{{FirstName}}", user.FirstName)
            .Replace("{{Password}}", tempPassword);

        var attachments = new List<(string path, string? contentId)>
        {
           (Path.Combine(AppContext.BaseDirectory, "wwwroot", "icons", "Krief-white-logo.png"), "Krief-white-logo.png"),
        };

        await _emailService.SendEmailAsync(
            user.Email,
            "Reset Password - TrackAI",
            html,
            attachments
        );
    }
}
