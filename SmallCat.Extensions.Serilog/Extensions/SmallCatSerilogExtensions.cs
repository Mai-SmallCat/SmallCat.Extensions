using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SmallCat.Extensions.Serilog;

public static class SmallCatSerilogExtensions
{
    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} <{ThreadId:00}> [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static WebApplicationBuilder AddSmallCatSerilog(this WebApplicationBuilder webApplicationBuilder, string logFilePath = "logs", string logFileName = "app")
    {
        var logPath = Path.Combine(logFilePath, $"{logFileName}.log");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            logPath = Path.Combine($"/var/log/{AppDomain.CurrentDomain.FriendlyName}", logFileName);
        }

        Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .Enrich.WithThreadId()
                     .Enrich.FromLogContext()
                     .WriteTo.Console(outputTemplate: OutputTemplate)
                     .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, outputTemplate: OutputTemplate)
                     .CreateLogger();

        webApplicationBuilder.Host.UseSerilog(Log.Logger, dispose: true);

        return webApplicationBuilder;
    }
}