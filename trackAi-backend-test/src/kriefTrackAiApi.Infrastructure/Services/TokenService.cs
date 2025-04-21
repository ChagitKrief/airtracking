




// using System;
// using System.Net.Http;
// using System.Text;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Configuration;
// using kriefTrackAiApi.Common.Dto;

// namespace kriefTrackAiApi.Infrastructure.Services;

// public class TokenService
// {
//     private readonly HttpClient _httpClient;
//     private readonly string _baseUrl;
//     private readonly string _clientId;
//     private readonly string _clientSecret;
//     private readonly IHttpContextAccessor _httpContextAccessor;

//     public TokenService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
//     {
//         _httpClient = new HttpClient();
//         _httpContextAccessor = httpContextAccessor;
//         _baseUrl = configuration["WinwordApi:BaseUrl"] ?? throw new ArgumentNullException("WinwordApi:BaseUrl is missing");
//         _clientId = configuration["WinwordApi:ClientId"] ?? throw new ArgumentNullException("WinwordApi:ClientId is missing");
//         _clientSecret = configuration["WinwordApi:ClientSecret"] ?? throw new ArgumentNullException("WinwordApi:ClientSecret is missing");
//     }

//     public async Task<string> GetGraphQLTokenAsync()
//     {
//         var context = _httpContextAccessor.HttpContext;
//         if (context != null && context.Request.Cookies.TryGetValue("WinwordToken", out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
//         {
//             return cachedToken;
//         }

//         Console.WriteLine("Requesting GraphQL Token...");

//         var queryObject = new
//         {
//             query = $"mutation {{ publicAPIToken(clientId: \"{_clientId}\", clientSecret: \"{_clientSecret}\") }}"
//         };

//         string jsonQuery = JsonSerializer.Serialize(queryObject);
//         var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");
//         HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl, content);

//         if (!response.IsSuccessStatusCode)
//         {
//             Console.WriteLine($"Token request failed: {response.StatusCode}");
//             var errorResponse = await response.Content.ReadAsStringAsync();
//             Console.WriteLine($"Token API Error Response: {errorResponse}");
//             return string.Empty;
//         }

//         var result = await response.Content.ReadAsStringAsync();
//         var tokenData = JsonSerializer.Deserialize<TokenRoot>(result);
//         string token = tokenData?.data?.publicAPIToken ?? string.Empty;

//         if (context != null && !string.IsNullOrEmpty(token))
//         {
//             context.Response.Cookies.Append("WinwordToken", token, new CookieOptions
//             {
//                 Expires = DateTimeOffset.Now.AddHours(24),
//                 HttpOnly = true,
//                 Secure = true
//             });
//         }

//         return token;
//     }
// }



using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using kriefTrackAiApi.Common.Dto;

namespace kriefTrackAiApi.Infrastructure.Services;

public class TokenService : IHostedService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<TokenService> _logger;
    private Timer? _timer;
    private const string TokenCacheKey = "WinwordToken";

    public TokenService(IConfiguration configuration, IMemoryCache memoryCache, ILogger<TokenService> logger)
    {
        _httpClient = new HttpClient();
        _memoryCache = memoryCache;
        _logger = logger;
        _baseUrl = configuration["WinwordApi:BaseUrl"] ?? throw new ArgumentNullException("WinwordApi:BaseUrl is missing");
        _clientId = configuration["WinwordApi:ClientId"] ?? throw new ArgumentNullException("WinwordApi:ClientId is missing");
        _clientSecret = configuration["WinwordApi:ClientSecret"] ?? throw new ArgumentNullException("WinwordApi:ClientSecret is missing");
    }

    public async Task<string> GetGraphQLTokenAsync()
    {
        if (_memoryCache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            return cachedToken;
        }

        return await RequestNewTokenAsync();
    }

    private async Task<string> RequestNewTokenAsync()
    {
        _logger.LogInformation("Requesting new GraphQL Token...");

        var queryObject = new
        {
            query = $"mutation {{ publicAPIToken(clientId: \"{_clientId}\", clientSecret: \"{_clientSecret}\") }}"
        };

        string jsonQuery = JsonSerializer.Serialize(queryObject);
        var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Token request failed: {response.StatusCode}");
            var errorResponse = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Token API Error Response: {errorResponse}");
            return string.Empty;
        }

        var result = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<TokenRoot>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        string token = tokenData?.data?.publicAPIToken ?? string.Empty;

        if (!string.IsNullOrEmpty(token))
        {
            _memoryCache.Set(TokenCacheKey, token, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) 
            });

            _logger.LogInformation("New token acquired and cached.");
        }

        return token;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TokenService is starting...");
        _timer = new Timer(async _ => await RequestNewTokenAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TokenService is stopping...");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
