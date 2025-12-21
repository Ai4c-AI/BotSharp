# BotSharp.Plugin.Sandbox

Provides AIO Sandbox sidecar utilities so agents can safely execute isolated shell/file/browser/Jupyter operations via HTTP.

## Configuration

Configure in `appsettings.json`:

```json
"Sandbox": {
  "BaseUrl": "http://localhost:3000",
  "ApiKeyHeader": "Authorization",
  "ApiKey": "Bearer <token>",
  "ResponseMaxLength": 6000
}
```

Enable the plugin in `PluginLoader:Assemblies` (already added in defaults).

## Exposed utility functions

- `util-sandbox-get_context`
- `util-sandbox-shell_exec`
- `util-sandbox-shell_wait`
- `util-sandbox-file_read`
- `util-sandbox-file_write`
- `util-sandbox-file_list`
- `util-sandbox-file_search`
- `util-sandbox-file_editor`
- `util-sandbox-browser_screenshot`
- `util-sandbox-jupyter_exec`
- `util-sandbox-jupyter_create_session`
- `util-sandbox-check_packages`
