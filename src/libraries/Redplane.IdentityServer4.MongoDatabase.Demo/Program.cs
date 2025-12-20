using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Redplane.IdentityServer4.MongoDatabase.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configurationBuilder)
                .WriteTo.Console()
                .WriteTo.File("logs/authentication.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger = logger;

            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseConfiguration(configurationBuilder)
                        .UseStartup<Startup>();
                });
        }
    }
}