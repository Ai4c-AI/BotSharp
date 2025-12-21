namespace BotSharp.Plugin.Sandbox.Settings;

public class SandboxSettings
{
    public string BaseUrl { get; set; } = "http://localhost:3000";

    public string ApiKey { get; set; } = string.Empty;

    public string ApiKeyHeader { get; set; } = "Authorization";

    public int ResponseMaxLength { get; set; } = 6000;
}
