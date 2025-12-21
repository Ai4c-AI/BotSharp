namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxJupyterCreateSessionFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.JupyterCreateSession;
    public string Indication => "Creating sandbox Jupyter session.";

    private readonly SandboxApiClient _client;

    public SandboxJupyterCreateSessionFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/jupyter/sessions/create", message.FunctionArgs);
        return true;
    }
}
