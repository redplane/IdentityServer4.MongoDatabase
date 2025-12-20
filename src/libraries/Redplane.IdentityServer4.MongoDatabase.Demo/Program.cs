using System;
using System.IO;
#if NETCOREAPP2_2 || NETCOREAPP3_0
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
#elif NET9_0_OR_GREATER
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
#endif
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Redplane.IdentityServer4.MongoDatabase.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if NETCOREAPP2_2 || NETCOREAPP3_0
            CreateWebHostBuilder(args).Build().Run();
#elif NET9_0_OR_GREATER
            CreateHostBuilder(args).Build().Run();
#endif
        }

#if NETCOREAPP2_2 || NETCOREAPP3_0
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
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

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configurationBuilder)
                .UseSerilog()
                .UseStartup<Startup>();
        }
#elif NET9_0_OR_GREATER
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
#endif
    }
}