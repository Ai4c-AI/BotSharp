namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxFileListFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.FileList;
    public string Indication => "Listing sandbox directory content.";

    private readonly SandboxApiClient _client;

    public SandboxFileListFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/file/list", message.FunctionArgs);
        return true;
    }
}
