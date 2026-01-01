using BotSharp.Plugin.PythonInterpreter.Helpers;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Plugin.PythonInterpreter.Services;

public class PythonScriptExecutor
{
    private readonly ILogger<PythonScriptExecutor> _logger;

    public PythonScriptExecutor(ILogger<PythonScriptExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExecuteScript(string scriptPath, string jsonArgs)
    {
        // 1. Environment preparation
        await EnsureDependencies(scriptPath);

        // 2. Execution
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        
        psi.ArgumentList.Add(scriptPath);
        psi.ArgumentList.Add("--json_args");
        psi.ArgumentList.Add(jsonArgs);

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start python process.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var output = await stdoutTask;
        var error = await stderrTask;

        if (process.ExitCode != 0)
        {
            _logger.LogError($"Python script execution failed. Exit code: {process.ExitCode}. Error: {error}");
            throw new Exception($"Script execution failed: {error}");
        }

        return output;
    }

    private async Task EnsureDependencies(string scriptPath)
    {
        var scriptDir = Path.GetDirectoryName(scriptPath);
        if (string.IsNullOrEmpty(scriptDir)) return;

        // Assuming structure: my-skill/scripts/analyze.py
        // requirements.txt should be in my-skill/
        var skillDir = Path.GetDirectoryName(scriptDir);
        if (string.IsNullOrEmpty(skillDir)) return;

        var requirementsPath = Path.Combine(skillDir, "requirements.txt");
        if (File.Exists(requirementsPath))
        {
            var result = await PyPackageHelper.InstallRequirementsFromFile(requirementsPath);
            if (!result.Success)
            {
                _logger.LogWarning($"Failed to install requirements from {requirementsPath}: {result.ErrorMsg}");
            }
        }
    }
}
