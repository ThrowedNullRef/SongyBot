namespace SongyBot;

public sealed class SongyConfig
{
    public SongyConfig(string botToken)
    {
        BotToken = botToken;
    }

    public string BotToken { get; }
}