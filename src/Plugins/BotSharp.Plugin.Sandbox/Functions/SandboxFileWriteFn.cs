namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxFileWriteFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.FileWrite;
    public string Indication => "Writing file content inside sandbox.";

    private readonly SandboxApiClient _client;

    public SandboxFileWriteFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/file/write", message.FunctionArgs);
        return true;
    }
}
