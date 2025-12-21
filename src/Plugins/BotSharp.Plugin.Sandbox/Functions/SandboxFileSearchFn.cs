namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxFileSearchFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.FileSearch;
    public string Indication => "Searching text inside sandbox files.";

    private readonly SandboxApiClient _client;

    public SandboxFileSearchFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/file/search", message.FunctionArgs);
        return true;
    }
}
