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
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Core.Models;

namespace kriefTrackAiApi.Infrastructure.Services;

public class WinwordQueryService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _dbContext;

    public WinwordQueryService(TokenService tokenService, AppDbContext dbContext)
    {
        _httpClient = new HttpClient();
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<List<DataItem>> FetchAndFilterShipmentDataAsync(int[] customerNumbers)
    {
        try
        {
            Console.WriteLine("Fetching shipment data with filtering...");

            string token = await _tokenService.GetGraphQLTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Error: No token received from WINWORD.");
                return new List<DataItem>();
            }

            var shipmentIdsFromDB = await _dbContext.ShipmentCustomers
                .Where(sc => customerNumbers.Contains(sc.CustomerNumber))
                .Select(sc => sc.ShipmentId)
                .ToListAsync();

            if (shipmentIdsFromDB.Count == 0)
            {
                Console.WriteLine("No shipment IDs found for given customer numbers.");
                return new List<DataItem>();
            }

            Console.WriteLine($"Total shipment IDs from DB: {shipmentIdsFromDB.Count}");

            List<DataItem> shipments = await FetchShipmentDataAsync(token, shipmentIdsFromDB);

            Console.WriteLine($"Total filtered shipments: {shipments.Count}");
            return shipments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching shipment data: {ex.Message}");
            return new List<DataItem>();
        }
    }


    private async Task<List<DataItem>> FetchShipmentDataAsync(string token, List<string> shipmentIds)
    {
        try
        {
            Console.WriteLine("Fetching shipment data with filtering...");
            Console.WriteLine($"Token: {token}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine("🔹 Checking Shipment IDs before sending request:");
            foreach (var id in shipmentIds)
            {
                Console.WriteLine($" ID: {id}");
            }

            var queryObject = new
            {
                query = GraphQLQueries.GetTrackedShipmentsByIdsQuery,
                variables = new
                {
                    ids = shipmentIds  
                }
            };

            string jsonQuery = JsonSerializer.Serialize(queryObject);
            Console.WriteLine($"Request JSON: {jsonQuery}");

            var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("https://graphql.wnwd.com/", content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API request failed: {response.StatusCode}");
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error Response: {errorResponse}");
                return new List<DataItem>();
            }

            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            var rootData = JsonSerializer.Deserialize<Root>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (rootData?.data?.trackedShipmentsByIds != null)
            {
                Console.WriteLine($"Total shipments fetched: {rootData.data.trackedShipmentsByIds.Count}");
                return rootData.data.trackedShipmentsByIds;
            }

            return new List<DataItem>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching shipment data: {ex.Message}");
            return new List<DataItem>();
        }
    }

}
