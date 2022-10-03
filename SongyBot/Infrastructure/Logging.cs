using Discord;
using Serilog;
using Serilog.Events;

namespace SongyBot.Infrastructure;

public static class Logging
{
    public static ILogger CreateLogger() => new LoggerConfiguration().Enrich.FromLogContext()
                                                                     .WriteTo.Console()
                                                                     .MinimumLevel.Debug()
                                                                     .CreateLogger();

    public static void Log(ILogger logger, LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        logger.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
    }
}