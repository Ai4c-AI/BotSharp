namespace BotSharp.Plugin.Sandbox.Functions;

public class SandboxFileEditorFn : IFunctionCallback
{
    public string Name => SandboxFunctionNames.FileEditor;
    public string Indication => "Applying structured file edits in sandbox.";

    private readonly SandboxApiClient _client;

    public SandboxFileEditorFn(SandboxApiClient client)
    {
        _client = client;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        message.Content = await _client.PostAsync("/v1/file/str_replace_editor", message.FunctionArgs);
        return true;
    }
}
