using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Adapters.Facebook.TestBot
{
    public class Program
    {
        private static ILogger _logger;

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var loggerF = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("BlazorDiceRoller", LogLevel.Information)
                .AddConsole()
                .AddAzureWebAppDiagnostics()
                .AddFilter("Microsoft", LogLevel.Information)
                .AddFilter("System", LogLevel.Information);
            });
            _logger = loggerF.CreateLogger<Program>();

            host.Run();
            Program.WriteToLog("Debug 0 Main");
        }

        public static void WriteToLog(string message)
        {
            Program._logger.LogError(message);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    // clear default logging providers
                    logging.ClearProviders();

                    // add built-in providers manually, as needed 
                    logging.AddAzureWebAppDiagnostics();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventLog();
                    logging.AddEventSourceLogger();

                    // logging.AddTraceSource(sourceSwitchName); 
                });
    }
}
