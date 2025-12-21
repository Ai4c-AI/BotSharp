namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxShellExecFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.ShellExec;
    public string Indication => "Executing shell command inside the sandbox.";

    private readonly SandboxApiClient _client;

    public SandboxShellExecFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/shell/exec", message.FunctionArgs);
        return true;
    }
}
