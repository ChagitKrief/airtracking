using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using kriefTrackAiApi.Common.Dto;

namespace kriefTrackAiApi.Infrastructure.Services;

public class WinwordFilterService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public WinwordFilterService(TokenService tokenService)
    {
        _httpClient = new HttpClient();
        _tokenService = tokenService;
    }

    public async Task<string> FetchFilteredShipmentsAsync(List<string> fields, List<string> values, List<string> customerCodes)
    {
        try
        {
            Console.WriteLine($"Fetching WIN data with fields: [{string.Join(", ", fields)}], values: [{string.Join(", ", values)}], customers: {string.Join(", ", customerCodes)}");

            string token = await _tokenService.GetGraphQLTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Error: No token received.");
                return "{}";
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var orFilters = fields.Zip(values, (f, v) => new
            {
                field = f,
                values = new[] { v }
            }).ToArray();

            var queryObject = new
            {
                query = GraphQLQueries.GetTrackedShipmentsQuery,
                variables = new
                {
                    filterBy = new[] {
                        new {
                            or = orFilters
                        }
                    },
                    limit = 1000,
                    offset = 0
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(queryObject), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("https://graphql.wnwd.com/", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"WIN API Error: {errorResponse}");
                return "{}";
            }

            string rawResult = await response.Content.ReadAsStringAsync();

            // Deserialize for local filtering
            using var doc = JsonDocument.Parse(rawResult);
            var root = doc.RootElement;

            var filteredShipments = root
                .GetProperty("data")
                .GetProperty("trackedShipments")
                .GetProperty("data")
                .EnumerateArray()
                .Where(item =>
                {
                    if (item.TryGetProperty("metadata", out var metadata) &&
                        metadata.TryGetProperty("businessData", out var businessData) &&
                        businessData.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var entry in businessData.EnumerateArray())
                        {
                            if (entry.TryGetProperty("key", out var keyProp) &&
                                keyProp.GetString() == "Customer Code" &&
                                entry.TryGetProperty("value", out var valueProp))
                            {
                                var customerCode = valueProp.GetString();
                                if (!string.IsNullOrEmpty(customerCode) && customerCodes.Contains(customerCode))
                                    return true;
                            }
                        }
                    }
                    return false;
                })
                .ToArray();

            var shaped = new
            {
                data = new
                {
                    trackedShipments = new
                    {
                        total = filteredShipments.Length,
                        totalFiltered = filteredShipments.Length,
                        data = filteredShipments
                    }
                }
            };

            return JsonSerializer.Serialize(shaped);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception fetching WIN data: {ex.Message}");
            return "{}";
        }
    }
}
