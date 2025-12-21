namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxCheckPackagesFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.CheckPackages;
    public string Indication => "Checking installed sandbox packages.";

    private readonly SandboxApiClient _client;

    public SandboxCheckPackagesFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        var lang = SandboxPayloadHelper.ReadString(message.FunctionArgs, "lang");
        var endpoint = "/v1/sandbox/packages";
        if (!string.IsNullOrWhiteSpace(lang))
        {
            endpoint = $"{endpoint}/{Uri.EscapeDataString(lang)}";
        }

        message.Content = await _client.GetAsync(endpoint);
        return true;
    }
}
