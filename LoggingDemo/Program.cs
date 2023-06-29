using Adapters.filters;
using Grpc.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace LoggingDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FunctionsDebugger.Enable();

            var host = new HostBuilder()
                 .ConfigureAppConfiguration((hostContext, configBuilder) =>
                 {
                     configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                 })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddApplicationInsights(
                        configureTelemetryConfiguration: (config) => config.ConnectionString = "InstrumentationKey=d788ef8f-02b3-403c-be9c-15eb165af809;IngestionEndpoint=https://norwayeast-0.in.applicationinsights.azure.com/;LiveEndpoint=https://norwayeast.livediagnostics.monitor.azure.com/",
                        configureApplicationInsightsLoggerOptions: (options) => { options.IncludeScopes = true; }
                    );

                    // Capture all log-level entries from Startup
                  
                })
                .ConfigureFunctionsWorkerDefaults(builder =>
                {
                    builder.UseMiddleware<LoggingMiddleware>();
                })
                .Build();


            host.Run();
        }
    }
}
