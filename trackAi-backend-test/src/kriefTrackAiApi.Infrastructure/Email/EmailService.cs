
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using kriefTrackAiApi.Common.Dto;

namespace kriefTrackAiApi.Infrastructure.Email;

public class EmailService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    public EmailService(HttpClient httpClient, ILogger<EmailService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string htmlBody,
                                    List<(string path, string? contentId)>? attachmentPaths = null)
    {
        try
        {
            if (!IsValidEmail(recipientEmail))
            {
                _logger.LogError("Invalid email address: {recipientEmail}", recipientEmail);
                throw new Exception($"Invalid email address: {recipientEmail}");
            }

            List<EmailAttachment>? attachments = attachmentPaths?
                .Select(tuple => BuildAttachment(tuple.path, tuple.contentId))
                .Where(att => att is not null)
                .Select(att => att!)
                .ToList();

            if (attachments != null && attachments.Any())
            {
                _logger.LogInformation("Preparing to send email with {Count} attachments", attachments.Count);
                foreach (var att in attachments)
                {
                    _logger.LogInformation("Attachment: {Name}, CID: {CID}, Base64 Length: {Length}",
                        att.Name, att.ContentId, att.ContentBytes.Length);
                }
            }

            var emailPayload = new
            {
                to = recipientEmail,
                subject = subject,
                body = htmlBody,
                attachments = attachments
            };

            string endpointBase = _configuration["Email:EndpointBase"]
          ?? throw new ArgumentNullException("Email:EndpointBase is missing");

            var endpoint = (attachments != null && attachments.Any())
                ? $"{endpointBase.TrimEnd('/')}/trackai_track_email_with_att"
                : $"{endpointBase.TrimEnd('/')}/trackai_track_email";


            var response = await _httpClient.PostAsJsonAsync(endpoint, emailPayload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email successfully sent to {recipientEmail}", recipientEmail);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email. StatusCode: {StatusCode}, Error: {Error}", response.StatusCode, errorMessage);
                throw new Exception($"Failed to send email: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send email: {Message}", ex.Message);
            throw;
        }
    }

    private EmailAttachment? BuildAttachment(string filePath, string? contentId = null)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Attachment file not found: {Path}", filePath);
            return null;
        }

        var bytes = File.ReadAllBytes(filePath);
        return new EmailAttachment
        {
            Name = Path.GetFileName(filePath),
            ContentBytes = Convert.ToBase64String(bytes),
            ContentId = contentId
        };
    }


    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
