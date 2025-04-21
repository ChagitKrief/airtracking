using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Infrastructure.Data;
using System.Data;
using kriefTrackAiApi.Infrastructure.Services;
using kriefTrackAiApi.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace kriefTrackAiApi.Infrastructure.Email;

public class EmailNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private readonly AppDbContext _dbContext;
    private readonly WinwordQueryService _winwordQueryService;
    private readonly ICompanyRepository _companyRepository;

    public EmailNotificationService(AppDbContext dbContext, IConfiguration configuration, EmailService emailService, WinwordQueryService winwordQueryService, ICompanyRepository companyRepository)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _emailService = emailService;
        _winwordQueryService = winwordQueryService;
        _companyRepository = companyRepository;
    }

    public async Task SendShipmentEmailsAsync()
    {
        Console.WriteLine("Processing shipment emails...");

        var usersToEmail = GetUsersForToday(await _dbContext.Users.Where(u => u.IsActive).ToListAsync());

        if (!usersToEmail.Any())
        {
            Console.WriteLine("No users scheduled for today.");
            return;
        }

        bool isTestMode = _configuration.GetValue<bool>("EmailSettings:EnableTestMode");
        string? testEmail = _configuration.GetValue<string>("EmailSettings:TestEmail");

        foreach (var user in usersToEmail)
        {
            Console.WriteLine($"Processing user: {user.FirstName} {user.LastName}");

            var activeCustomers = await _companyRepository.GetActiveCustomersByIdsAsync(user.CompanyIds);
            var customerNumbers = activeCustomers.Select(c => c.CustomerNumber).Distinct().ToArray();

            if (!customerNumbers.Any())
            {
                Console.WriteLine($"No active companies found for user {user.FirstName} {user.LastName}.");
                continue;
            }

            var companyMap = activeCustomers.ToDictionary(c => c.CustomerNumber.ToString(), c => c.CustomerName);

            List<DataItem> shipmentData = await _winwordQueryService.FetchAndFilterShipmentDataAsync(customerNumbers);

            if (!shipmentData.Any())
            {
                Console.WriteLine($"No shipments found for user {user.FirstName} {user.LastName}.");
                continue;
            }

            var shipmentsByCustomerCode = shipmentData
                .Where(s => s.metadata?.businessData?.Any(b => b.key == "Customer Code") == true)
                .GroupBy(s => s.metadata?.businessData?.FirstOrDefault(b => b.key == "Customer Code")?.value ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var customerShipments in shipmentsByCustomerCode)
            {
                string customerCode = customerShipments.Key;
                List<DataItem> customerShipmentData = customerShipments.Value;

                string companyName = companyMap.TryGetValue(customerCode, out var name) ? name : $"Customer {customerCode}";

                Console.WriteLine($"Preparing email for user {user.FirstName} {user.LastName}, Company: {companyName} (Customer Code: {customerCode})");

                string emailBody = GenerateEmailHtml(customerShipmentData, $"{user.FirstName} {user.LastName}", companyName);
                string? recipientEmail = isTestMode ? testEmail : user.Email;

                if (!string.IsNullOrWhiteSpace(recipientEmail))
                {
                    Console.WriteLine($"Sending email to: {recipientEmail} for {companyName}");

                    await _emailService.SendEmailAsync(recipientEmail, $"Tracking Shipment Notification - {companyName}", emailBody);

                    await Task.Delay(1000);
                }
            }
        }
    }



    private List<User> GetUsersForToday(List<User> users)
    {
        string today = DateTime.UtcNow.DayOfWeek.ToString();
        return users.Where(u => u.Reminders != null && u.Reminders.Any(r => r.Equals(today, StringComparison.OrdinalIgnoreCase))).ToList();
    }
    private string GenerateEmailHtml(List<DataItem> shipments, string userName, string companyName)
    {

        string filePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "email", "EmailTemplate.html");

        string htmlTemplate = File.Exists(filePath)
            ? File.ReadAllText(filePath)
            : "<h1>Template not found</h1>";


        htmlTemplate = htmlTemplate
            .Replace("placeholder_userName", userName)
            .Replace("placeholder_companyName", companyName)
            .Replace("placeholder_aIPredictedETAUpdates", shipments.Count.ToString());

        int onTime = shipments.Count(s => s.shipment?.status?.predicted?.diffFromCarrierDays == 0);
        int earlyArrivals = shipments.Count(s => s.shipment?.status?.predicted?.diffFromCarrierDays < 0);
        int significantDelays = shipments.Count(s => s.shipment?.status?.predicted?.diffFromCarrierDays is >= 1 and <= 4);
        int criticalDelays = shipments.Count(s => s.shipment?.status?.predicted?.diffFromCarrierDays >= 5);

        htmlTemplate = htmlTemplate
            .Replace("placeholder_onTime", onTime.ToString())
            .Replace("placeholder_earlyArrivals", earlyArrivals.ToString())
            .Replace("placeholder_significantDelays", significantDelays.ToString())
            .Replace("placeholder_criticalDelays", criticalDelays.ToString());

        DataTable dt = new DataTable();
        dt.Columns.Add("Container");
        dt.Columns.Add("File");
        dt.Columns.Add("Customer Reference");
        dt.Columns.Add("Latest Carrier ETA/ATA");
        dt.Columns.Add("ETA Change");
        dt.Columns.Add("Origin");
        dt.Columns.Add("Destination");
        dt.Columns.Add("Company");
        dt.Columns.Add("TimeColor");
        dt.Columns.Add("Map");

        foreach (var shipment in shipments)
        {
            var s = shipment.shipment;
            if (s == null) continue;

            DataRow dr = dt.NewRow();
            dr["Container"] = s.containerNumber ?? "N/A";
            dr["File"] = shipment.metadata?.jobNumber ?? "N/A";
            dr["Customer Reference"] = shipment.metadata?.businessData?.FirstOrDefault(b => b.key == "Customer Reference")?.value ?? "N/A";
            string? latestCarrierETAATA = s.status?.actualArrivalAt?.ToString("dd/MM/yyyy")
                ?? s.status?.estimatedArrivalAt?.ToString("dd/MM/yyyy")
                ?? (shipment.metadata?.eta != null ? DateTime.Parse(shipment.metadata.eta).ToUniversalTime().ToString("dd/MM/yyyy") : "N/A");

            dr["Latest Carrier ETA/ATA"] = latestCarrierETAATA;
            dr["ETA Change"] = s.status?.predicted?.diffFromCarrierDays?.ToString() ?? "N/A";
            bool hasOrigin = shipment.metadata?.businessData?.Any(b => b.key == "Origin  Country") ?? false;
            string origin = hasOrigin
                ? shipment.metadata?.businessData?.FirstOrDefault(b => b.key == "Origin  Country")?.value ?? "Unknown"
                : _configuration.GetValue<string>("DynamicData:DefaultShipmentOrigin") ?? "Unknown";
            dr["Origin"] = origin;
            dr["Destination"] = s.status?.pod?.properties?.name ?? "Unknown";
            dr["Company"] = s.carrier?.shortName ?? "N/A";
            dr["TimeColor"] = GetEtaStyle(dr["ETA Change"]?.ToString() ?? "N/A");

            string baseTrackingUrl = _configuration["AppSettings:TrackingWrapperBaseUrl"] ?? throw new Exception("TrackingWrapperBaseUrl missing");
            string wrapperUrl = $"{baseTrackingUrl}{s.id}?url={HttpUtility.UrlEncode(shipment.sharedShipmentLink)}";

            dr["Map"] = s.id != null
                ? $"<a href='{wrapperUrl}' class='map-button' target='_blank'>Map</a>"
                : "N/A";

            dt.Rows.Add(dr);
        }

        string shipmentTable = ConvertToHtml(dt);

        return htmlTemplate.Replace("placeholder_shipmentTable", shipmentTable);
    }

    private static string ConvertToHtml(DataTable dt)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<table class='shipmentTableWrapper'>");
        sb.AppendLine("<thead><tr>");
        foreach (DataColumn column in dt.Columns)
        {
            if (column.ColumnName != "TimeColor")
            {
                sb.AppendFormat("<th class='tableHeader'>{0}</th>", column.ColumnName);
            }
        }
        sb.AppendLine("</tr></thead>");
        sb.AppendLine("<tbody>");

        foreach (DataRow row in dt.Rows)
        {
            string rowClass = dt.Rows.IndexOf(row) % 2 == 0 ? "evenRow" : "oddRow";
            sb.Append($"<tr class='{rowClass}'>");
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName == "ETA Change")
                {
                    // string etaStyle = row["TimeColor"]?.ToString() ?? "etaStyleUnknown";
                    string etaStyle = row["TimeColor"]?.ToString() ?? "etaStyleUnknown";
                    string etaText = row[column]?.ToString() ?? "N/A";

                    if (etaText != "N/A" && int.TryParse(etaText, out int diffDays))
                    {
                        if (diffDays == 0)
                            etaText = "On Time";
                        else if (diffDays < 0)
                            etaText = $"Early {Math.Abs(diffDays)} + days";
                        else if (diffDays >= 1 && diffDays <= 4)
                            etaText = $" {diffDays} - delay";
                        else if (diffDays >= 5)
                            etaText = $"  delay {diffDays}+ days";
                    }
                    sb.AppendFormat("<td class='{0} etaStyle'>{1}</td>", etaStyle, etaText);
                }
                else if (column.ColumnName != "TimeColor")
                {
                    sb.AppendFormat("<td>{0}</td>", row[column]);
                }
            }
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table>");
        return sb.ToString();
    }
    private string GetEtaStyle(string etaChange)
    {
        if (int.TryParse(etaChange, out int diff))
        {
            if (diff == 0) return "etaStyleOnTime";
            if (diff < 0) return "etaStyleEarly";
            if (diff is >= 1 and <= 4) return "etaStyleSignificantDelay";
            if (diff >= 5) return "etaStyleCriticalDelay";
        }
        return "";
    }

}
