using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Redplane.IdentityServer4.MongoDatabase.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Startup>()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configurationBuilder)
                .WriteTo.Console()
                .WriteTo.File("logs/authentication.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger = logger;

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configurationBuilder)
                .UseSerilog()
                .UseStartup<Startup>();
        }
    }
}