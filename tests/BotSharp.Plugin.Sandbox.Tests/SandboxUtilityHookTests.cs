using System.Collections.Generic;
using System.Linq;
using BotSharp.Abstraction.Agents.Models;
using BotSharp.Plugin.Sandbox.Hooks;
using BotSharp.Plugin.Sandbox.Models;

namespace BotSharp.Plugin.Sandbox.Tests;

[TestClass]
public class SandboxUtilityHookTests
{
    [TestMethod]
    public void AddUtilities_ShouldExposeAllSandboxFunctions()
    {
        var utilities = new List<AgentUtility>();
        var hook = new SandboxUtilityHook();

        hook.AddUtilities(utilities);

        Assert.AreEqual(1, utilities.Count);
        var functionNames = utilities.Single().Items.Select(x => x.FunctionName).ToList();

        var expected = new[]
        {
            SandboxFunctionNames.GetContext,
            SandboxFunctionNames.ShellExec,
            SandboxFunctionNames.ShellWait,
            SandboxFunctionNames.FileRead,
            SandboxFunctionNames.FileWrite,
            SandboxFunctionNames.FileList,
            SandboxFunctionNames.FileSearch,
            SandboxFunctionNames.FileEditor,
            SandboxFunctionNames.BrowserScreenshot,
            SandboxFunctionNames.JupyterExec,
            SandboxFunctionNames.JupyterCreateSession,
            SandboxFunctionNames.CheckPackages
        };

        CollectionAssert.AreEquivalent(expected, functionNames);
    }
}
