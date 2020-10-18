using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace ECommerce.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                                .Enrich.FromLogContext()
                                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                                .WriteTo.Console() // Serilog will write logs to the console.
                                .WriteTo.File(new JsonFormatter(), // Serilog will write logs to the log file
                                                "Logs/Log-.json",
                                                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                                                rollingInterval: RollingInterval.Day)
                                .CreateLogger(); // Creates a Serilog logger instance on the static Log.Logger property

            try
            {
                var host = CreateHostBuilder(args).Build();

                var environment = host.Services.GetRequiredService<IHostEnvironment>();
                SwitchLoggingToAzureApplicationInsightForProduction(environment);

                Log.Information("API Server starting up");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Oops ... API Server terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // Registers the SerilogLoggerFactory and connects the Log.Logger as the sole logging provider
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void SwitchLoggingToAzureApplicationInsightForProduction(IHostEnvironment environment)
        {
            var logConfiguration = new LoggerConfiguration()
                                        .Enrich.FromLogContext()
                                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);

            if (!environment.IsDevelopment())
            {
                logConfiguration.WriteTo.ApplicationInsights(new TraceTelemetryConverter(),
                                                                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
            }

            Log.Logger = logConfiguration.CreateLogger();
        }
    }
}
