using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class BlazorPrerenderService
{
    private readonly IConfiguration _config;

    public BlazorPrerenderService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> PrerenderPageAsync(string relativePath)
    {
        var baseUrl = _config["SSR:BaseUrl"]
            ?? throw new ArgumentNullException("SSR:BaseUrl is missing");
        var url = $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";

        Console.WriteLine($"Rendering page: {url}");

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

            var context = await browser.NewContextAsync(new()
            {
                UserAgent = "TrackAi-SSR-Agent",
                ExtraHTTPHeaders = new Dictionary<string, string>
                {
                    { "X-SSR-Request", "true" }
                }
            });

            var page = await context.NewPageAsync();

            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = 60000,
                WaitUntil = WaitUntilState.Load
            });

            var content = await page.ContentAsync();
            Console.WriteLine($"Render complete for: {url}");

            return content;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to render page: {url}");
            Console.Error.WriteLine(ex);
            throw;
        }
    }
}
