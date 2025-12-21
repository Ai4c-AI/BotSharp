namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxFileReadFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.FileRead;
    public string Indication => "Reading file content from sandbox.";

    private readonly SandboxApiClient _client;

    public SandboxFileReadFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/file/read", message.FunctionArgs);
        return true;
    }
}
