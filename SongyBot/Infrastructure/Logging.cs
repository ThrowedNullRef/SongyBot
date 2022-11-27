using Discord;
using Serilog;
using Serilog.Events;

namespace SongyBot.Infrastructure;

public static class Logging
{
    public static ILogger CreateLogger() => new LoggerConfiguration().Enrich.FromLogContext()
                                                                     .WriteTo.Console()
                                                                     .MinimumLevel.Information()
                                                                     .CreateLogger();

    public static void Log(ILogger logger, LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Information,
            LogSeverity.Debug => LogEventLevel.Information,
            _ => LogEventLevel.Information
        };

        logger.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
    }
}