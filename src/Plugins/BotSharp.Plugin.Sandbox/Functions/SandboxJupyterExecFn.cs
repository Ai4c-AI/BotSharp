namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxJupyterExecFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.JupyterExec;
    public string Indication => "Running code via sandbox Jupyter runtime.";

    private readonly SandboxApiClient _client;

    public SandboxJupyterExecFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/jupyter/execute", message.FunctionArgs);
        return true;
    }
}
