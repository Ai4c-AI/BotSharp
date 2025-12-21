namespace BotSharp.Plugin.Sandbox.Models;

public static class SandboxFunctionNames
{
    public const string GetContext = "util-sandbox-get_context";
    public const string ShellExec = "util-sandbox-shell_exec";
    public const string ShellWait = "util-sandbox-shell_wait";
    public const string FileRead = "util-sandbox-file_read";
    public const string FileWrite = "util-sandbox-file_write";
    public const string FileList = "util-sandbox-file_list";
    public const string FileSearch = "util-sandbox-file_search";
    public const string FileEditor = "util-sandbox-file_editor";
    public const string BrowserScreenshot = "util-sandbox-browser_screenshot";
    public const string JupyterExec = "util-sandbox-jupyter_exec";
    public const string JupyterCreateSession = "util-sandbox-jupyter_create_session";
    public const string CheckPackages = "util-sandbox-check_packages";
}

public static class SandboxPayloadHelper
{
    public static string ReadString(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var node))
            {
                return node.ValueKind == JsonValueKind.String ? node.GetString() ?? string.Empty : node.ToString();
            }
        }
        catch
        {
        }

        return string.Empty;
    }
}
