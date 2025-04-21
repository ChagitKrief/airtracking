using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Infrastructure.Data;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Infrastructure.Services;
using kriefTrackAiApi.Core.Interfaces;
using System.Web;

namespace kriefTrackAiApi.Infrastructure.SmsService;

public class SmsNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SmsNotificationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly WinwordQueryService _winwordQueryService;
    private readonly ICompanyRepository _companyRepository;

    public SmsNotificationService(AppDbContext dbContext, IConfiguration configuration, ILogger<SmsNotificationService> logger, WinwordQueryService winwordQueryService, ICompanyRepository companyRepository)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
        _winwordQueryService = winwordQueryService;
        _companyRepository = companyRepository;
    }

    public async Task SendShipmentSmsAsync()
    {
        Console.WriteLine("Processing shipment SMS...");

        var usersToSms = _dbContext.Users.Where(u => u.IsActive).ToList();
        if (!usersToSms.Any())
        {
            Console.WriteLine("No active users found.");
            return;
        }

        bool isTestMode = _configuration.GetValue<bool>("SMS:EnableTestMode");
        string? testPhoneNumber = _configuration["SMS:TestPhoneNumber"];

        foreach (var user in usersToSms)
        {
            Console.WriteLine($"Processing user: {user.FirstName} {user.LastName}");

            var activeCustomers = await _companyRepository.GetActiveCustomersByIdsAsync(user.CompanyIds);
            var customerNumbers = activeCustomers.Select(c => c.CustomerNumber).Distinct().ToArray();

            if (!customerNumbers.Any())
            {
                Console.WriteLine($"No active companies found for user {user.FirstName} {user.LastName}.");
                continue;
            }

            List<DataItem> shipmentData = await _winwordQueryService.FetchAndFilterShipmentDataAsync(customerNumbers);

            if (!shipmentData.Any())
            {
                Console.WriteLine($"No shipments found for user {user.FirstName} {user.LastName}.");
                continue;
            }

            string recipientPhone = isTestMode ? testPhoneNumber ?? "" : user.Phone ?? "";
            if (string.IsNullOrWhiteSpace(recipientPhone))
            {
                Console.WriteLine($"Skipping SMS for {user.FirstName} {user.LastName} - No phone number.");
                continue;
            }

            foreach (var shipment in shipmentData)
            {
                List<string> smsMessages = GenerateSmsMessages(new List<DataItem> { shipment }, user.FirstName);

                Console.WriteLine($"Sending SMS to: {recipientPhone} for container {shipment.shipment?.containerNumber ?? "Unknown"}");
                await SendSmsAsync(recipientPhone, smsMessages);

                await Task.Delay(1000);
            }
        }
    }

    private async Task<bool> SendSmsAsync(string recipientPhone, List<string> messages)
    {
        if (string.IsNullOrWhiteSpace(recipientPhone))
        {
            _logger.LogWarning("Skipping SMS - No recipient phone number provided.");
            return false;
        }

        string apiUrl = _configuration["SMS:APIUrl"]!;
        string apiUser = _configuration["SMS:APIUserName"]!;
        string apiPassword = _configuration["SMS:APIUserPassword"]!;
        string sender = _configuration["SMS:SenderNameOrNumber"]!;
        bool isTestMode = _configuration.GetValue<bool>("SMS:EnableTestMode");
        string? testPhoneNumber = _configuration["SMS:TestPhoneNumber"];

        string finalRecipientPhone = isTestMode ? testPhoneNumber ?? recipientPhone : recipientPhone;

        _logger.LogInformation($"Preparing {messages.Count} SMS messages for {finalRecipientPhone}");

        foreach (var smsMessage in messages)
        {
            var requestData = new Dictionary<string, string>
              {
                  { "service", "send_sms" },
                  { "message", smsMessage },
                  { "dest", finalRecipientPhone },
                  { "sender", sender },
                  { "username", apiUser },
                  { "password", apiPassword }
              };

            var content = new FormUrlEncodedContent(requestData);

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"SMS sent successfully to {finalRecipientPhone}");
                    _logger.LogInformation($"Response: {responseBody}");
                    await Task.Delay(1000);
                }
                else
                {
                    _logger.LogError($"SMS sending failed for {finalRecipientPhone}: {response.StatusCode}");
                    _logger.LogError($"Response: {responseBody}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending SMS to {finalRecipientPhone}: {ex.Message}");
                return false;
            }
        }

        return true;
    }

    private List<string> GenerateSmsMessages(List<DataItem> shipments, string userName)
    {
        List<string> messages = new List<string>();

        foreach (var shipment in shipments)
        {
            var s = shipment.shipment;
            if (s == null) continue;

            StringBuilder smsText = new StringBuilder();
            smsText.AppendLine($"Hi {userName}, your shipment updates:");

            string container = s.containerNumber ?? "Unknown";
            string jobOrder = "N/A";
            string fileNo = shipment.metadata?.jobNumber ?? "N/A";
            string supplier = shipment.metadata?.businessData?.FirstOrDefault(b => b.key == "Supplier Name")?.value ?? "N/A";
            string origin = s.status?.pol?.properties?.name ?? "Unknown";
            string destination = shipment.metadata?.businessData?.FirstOrDefault(b => b.key == "Discharge Port")?.value ?? "Unknown";
            string eta = s.status?.estimatedArrivalAt?.ToString("dd/MM/yyyy") ??
                         (DateTime.TryParse(s.initialCarrierETA, out DateTime parsedDate) ? parsedDate.ToString("dd/MM/yyyy") : "N/A");
            string wrapperBase = _configuration["AppSettings:TrackingWrapperBaseUrl"]
                ?? throw new ArgumentNullException("AppSettings:TrackingWrapperBaseUrl is missing");

            string trackingLink = s.id != null
                ? $"Tracking: {wrapperBase}{s.id}?url={HttpUtility.UrlEncode(shipment.sharedShipmentLink)}"
                : "Tracking: Not Available";

            smsText.AppendLine($"Container: {container}, Job: {jobOrder}, File: {fileNo}, Supplier: {supplier}, From: {origin} → {destination}, ETA: {eta}. {trackingLink}");

            messages.Add(smsText.ToString());
        }

        return messages;
    }
}
