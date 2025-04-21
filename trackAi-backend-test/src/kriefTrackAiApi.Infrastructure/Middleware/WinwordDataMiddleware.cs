using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using kriefTrackAiApi.Infrastructure.Data;
using kriefTrackAiApi.Infrastructure.Services;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;
// using kriefTrackAiApi.Infrastructure.Email;
// using kriefTrackAiApi.Infrastructure.SmsService;

namespace kriefTrackAiApi.Infrastructure.Middleware;

public class WinwordDataMiddleware : IWinwordRepository
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public WinwordDataMiddleware(AppDbContext dbContext, TokenService tokenService)
    {
        _dbContext = dbContext;
        _httpClient = new HttpClient();
        _tokenService = tokenService;
    }


    public async Task<List<DataItem>> FetchShipmentDataAsync(string token)
    {
        try
        {
            Console.WriteLine("Fetching shipment data...");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<DataItem> allShipments = new List<DataItem>();
            int offset = 0;
            const int batchSize = 1000;
            bool hasMoreData = true;

            while (hasMoreData)
            {
                var queryObject = new
                {
                    query = GraphQLQueries.GetTrackedShipmentsQuery,
                    variables = new
                    {
                        offset = offset,
                        limit = batchSize,
                        sort = new object[] { },
                        orderBy = new object[]
                        {
                        new { field = "shipment_predicted_datetime", order = "desc" }
                        },
                        filter = new
                        {
                            showAISEvents = true
                        },
                        options = new
                        {
                            preferAISEventsOverCarrier = true,
                            mergeAISPortCalls = true
                        },
                        searchTerm = ""
                    }
                };

                string jsonQuery = JsonSerializer.Serialize(queryObject);
                var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync("https://graphql.wnwd.com/", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API request failed: {response.StatusCode}");
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error Response: {errorResponse}");
                    return allShipments;
                }

                var result = await response.Content.ReadAsStringAsync();
                var rootData = JsonSerializer.Deserialize<Root>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (rootData?.data?.trackedShipments?.data != null)
                {
                    allShipments.AddRange(rootData.data.trackedShipments.data);
                    Console.WriteLine($"Fetched {rootData.data.trackedShipments.data.Count} shipments with offset {offset}");

                    if (rootData.data.trackedShipments.data.Count < batchSize)
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        offset += batchSize;
                    }
                }
                else
                {
                    hasMoreData = false;
                }
            }

            Console.WriteLine($"Total shipments fetched: {allShipments.Count}");
            return allShipments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching shipment data: {ex.Message}");
            return new List<DataItem>();
        }
    }

    public async Task FetchAndSaveSmsDataAsync()
    {
        try
        {
            Console.WriteLine("FetchAndSaveSmsData started...");

            string token = await _tokenService.GetGraphQLTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Error: No token received.");
                return;
            }

            List<DataItem> shipmentData = await FetchShipmentDataAsync(token);
            if (shipmentData == null || shipmentData.Count == 0)
            {
                Console.WriteLine("Error: No valid shipment data received.");
                return;
            }

            Console.WriteLine($"Shipment data received: {shipmentData.Count} records found.");

            if (shipmentData.Count > 0)
            {
                // _dbContext.SmsMessages.RemoveRange(_dbContext.SmsMessages);
                _dbContext.ShipmentCustomers.RemoveRange(_dbContext.ShipmentCustomers);
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.SaveChangesAsync();

            int smsRecordsAdded = 0;
            int shipmentCustomersAdded = 0;
            foreach (var item in shipmentData)
            {
                if (item.shipment == null)
                {
                    Console.WriteLine($"Warning: Missing shipment data for item ID: {item.id}");
                    continue;
                }

                if (string.IsNullOrEmpty(item.shipment.containerNumber) || string.IsNullOrEmpty(item.shipment.id))
                {
                    Console.WriteLine($"Warning: Shipment data missing fields for item ID: {item.id}");
                    continue;
                }

                var customerCodeEntry = item.metadata?.businessData?.FirstOrDefault(b => b.key == "Customer Code");

                if (customerCodeEntry != null && int.TryParse(customerCodeEntry.value, out int customerCode))
                {
                    string mobileNumber = ExtractMobileNumber(customerCode);
                    if (!string.IsNullOrEmpty(mobileNumber))
                    {
                        // var smsRecord = new Sms
                        // {
                        //     Id = Guid.NewGuid(),
                        //     Container = item.shipment.containerNumber ?? "Unknown",
                        //     MobileList = mobileNumber,
                        //     ShipmentId = item.shipment.id ?? "Unknown"
                        // };

                        // _dbContext.SmsMessages.Add(smsRecord);
                        smsRecordsAdded++;
                    }

                    var shipmentCustomerRecord = new ShipmentCustomers
                    {
                        Id = Guid.NewGuid(),
                        ShipmentId = item.id ?? "Unknown",
                        CustomerNumber = customerCode
                    };

                    _dbContext.ShipmentCustomers.Add(shipmentCustomerRecord);
                    shipmentCustomersAdded++;
                }
            }

            if (smsRecordsAdded > 0 || shipmentCustomersAdded > 0)
            {
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"{smsRecordsAdded} SMS records saved.");
                Console.WriteLine($"{shipmentCustomersAdded} ShipmentCustomers records saved.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
    private string ExtractMobileNumber(int customerCode)
    {
        var companyWithUser = _dbContext.Companies
            .Join(_dbContext.Users, c => c.Id, u => u.CompanyIds.FirstOrDefault(), (c, u) => new { c, u })
            .Where(x => x.c.CustomerNumber == customerCode && x.u.IsActive)
            .Select(x => x.u.Phone ?? x.u.Email)
            .FirstOrDefault();

        return companyWithUser ?? string.Empty;
    }
}
