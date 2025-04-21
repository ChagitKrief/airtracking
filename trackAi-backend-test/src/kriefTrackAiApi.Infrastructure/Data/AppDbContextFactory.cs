using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace kriefTrackAiApi.Infrastructure.Data;

  public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
  {
      public AppDbContext CreateDbContext(string[] args)
      {
          var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../kriefTrackAiApi.Web");
          Console.WriteLine($"Base Path: {basePath}");
          Console.WriteLine($"Looking for: {Path.Combine(basePath, "appsettings.json")}");

          if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
          {
              Console.WriteLine("appsettings.json not found!");
              throw new FileNotFoundException("appsettings.json was not found!", Path.Combine(basePath, "appsettings.json"));
          }

          var config = new ConfigurationBuilder()
              .SetBasePath(basePath)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .Build();

          var connectionString = config.GetConnectionString("DefaultConnection");

          if (string.IsNullOrEmpty(connectionString))
          {
              Console.WriteLine(" Connection string is missing!");
              throw new ArgumentNullException(nameof(connectionString), "Connection string is missing!");
          }

          Console.WriteLine($"Connection string found: {connectionString}");

          var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
          optionsBuilder.UseNpgsql(connectionString);

          return new AppDbContext(optionsBuilder.Options);
      }
  }
