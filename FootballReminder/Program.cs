using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using FootballReminder.Services;
using System.Threading.Tasks;

namespace FootballReminder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .WriteTo.File($"{Directory.GetCurrentDirectory()}/info.log",
                outputTemplate: "[{Timestamp}] {Level:u3}: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IFootballDataProvider, ApiFootball>();
                    services.AddSingleton<ICalendarService, GoogleCalendar>();
                    services.AddSingleton<IFootballReminderService, FootballReminderService>();
                })
                .UseSerilog()
                .Build();

            var service = ActivatorUtilities.CreateInstance<FootballReminderService>(host.Services);
            await service.Run();
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath($"{Directory.GetCurrentDirectory()}")
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}

