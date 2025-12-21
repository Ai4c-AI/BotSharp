namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxBrowserScreenshotFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.BrowserScreenshot;
    public string Indication => "Capturing sandbox browser screenshot.";

    private readonly SandboxApiClient _client;

    public SandboxBrowserScreenshotFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/browser/screenshot", message.FunctionArgs);
        return true;
    }
}
