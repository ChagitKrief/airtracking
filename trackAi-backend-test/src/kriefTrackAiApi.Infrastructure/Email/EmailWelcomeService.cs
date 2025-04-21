using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using kriefTrackAiApi.Core.Models;

namespace kriefTrackAiApi.Infrastructure.Email;

public class EmailWelcomeService
{
    private readonly EmailService _emailService;

    public EmailWelcomeService(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<string> BuildWelcomeEmailHtmlAsync(User user, string plainPassword)
    {
        string htmlTemplatePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "email", "WelcomeEmailTemplate.html");

        string htmlTemplate = File.Exists(htmlTemplatePath)
            ? await File.ReadAllTextAsync(htmlTemplatePath)
            : "<h1>Template not found</h1>";

        return htmlTemplate
            .Replace("{{FirstName}}", user.FirstName)
            .Replace("{{Email}}", user.Email)
            .Replace("{{Password}}", plainPassword);
    }

    public async Task SendWelcomeEmailAsync(User user, string plainPassword)
    {
        var html = await BuildWelcomeEmailHtmlAsync(user, plainPassword);

        var attachments = new List<(string path, string? contentId)>
        {
           (Path.Combine(AppContext.BaseDirectory, "wwwroot", "icons", "Krief-white-logo.png"), "Krief-white-logo.png"),
            (Path.Combine(AppContext.BaseDirectory, "wwwroot", "icons", "krief_welcome.gif"), "krief_welcome.gif")
        };

        await _emailService.SendEmailAsync(
            user.Email,
            "Welcome to the tracking system",
            html,
            attachments
        );
    }
}
