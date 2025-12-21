namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxGetContextFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.GetContext;
    public string Indication => "Inspecting sandbox environment.";

    private readonly SandboxApiClient _client;

    public SandboxGetContextFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.GetAsync("/v1/sandbox");
        return true;
    }
}
