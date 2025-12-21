namespace BotSharp.Plugin.Sandbox.Hooks;

public class SandboxUtilityHook : IAgentUtilityHook
{
    public void AddUtilities(List<AgentUtility> utilities)
    {
        var items = new List<UtilityItem>
        {
            new() { FunctionName = SandboxFunctionNames.GetContext },
            new() { FunctionName = SandboxFunctionNames.ShellExec },
            new() { FunctionName = SandboxFunctionNames.ShellWait },
            new() { FunctionName = SandboxFunctionNames.FileRead },
            new() { FunctionName = SandboxFunctionNames.FileWrite },
            new() { FunctionName = SandboxFunctionNames.FileList },
            new() { FunctionName = SandboxFunctionNames.FileSearch },
            new() { FunctionName = SandboxFunctionNames.FileEditor },
            new() { FunctionName = SandboxFunctionNames.BrowserScreenshot },
            new() { FunctionName = SandboxFunctionNames.JupyterExec },
            new() { FunctionName = SandboxFunctionNames.JupyterCreateSession },
            new() { FunctionName = SandboxFunctionNames.CheckPackages }
        };

        utilities.Add(new AgentUtility
        {
            Category = "sandbox",
            Name = "aio_sandbox",
            Items = items
        });
    }
}
