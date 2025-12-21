namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxShellWaitFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.ShellWait;
    public string Indication => "Waiting for sandbox shell task completion.";

    private readonly SandboxApiClient _client;

    public SandboxShellWaitFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/shell/wait", message.FunctionArgs);
        return true;
    }
}
