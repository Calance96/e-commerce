using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;

namespace ECommerce.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .WriteTo.Console() // Serilog will write logs to the console.
                        .WriteTo.File(new JsonFormatter(), // Serilog will write logs to the log file
                                        "Logs/Info/IdentityServerInfoLog-.json",
                                        restrictedToMinimumLevel: LogEventLevel.Information,
                                        rollingInterval: RollingInterval.Day)
                        .WriteTo.File(new JsonFormatter(),
                                        "Logs/Error/IdentityServerErrorLog-.json",
                                        restrictedToMinimumLevel: LogEventLevel.Error,
                                        rollingInterval: RollingInterval.Day)
                        .CreateLogger(); // Creates a Serilog logger instance on the static Log.Logger property

            try
            {
                Log.Information("Identity Server spinning up...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal($"Error starting up Identity Server... | Exception: {ex}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}