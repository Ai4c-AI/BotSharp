namespace BotSharp.Plugin.Sandbox.Services;

public class SandboxApiClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly SandboxSettings _settings;
    private readonly ILogger<SandboxApiClient> _logger;

    public SandboxApiClient(IHttpClientFactory clientFactory,
        SandboxSettings settings,
        ILogger<SandboxApiClient> logger)
    {
        _clientFactory = clientFactory;
        _settings = settings;
        _logger = logger;
    }

    public Task<string> GetAsync(string path)
        => SendAsync(new HttpRequestMessage(HttpMethod.Get, BuildUri(path)));

    public Task<string> PostAsync(string path, string? payload)
    {
        var body = string.IsNullOrWhiteSpace(payload) ? "{}" : payload;
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path))
        {
            Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        return SendAsync(request);
    }

    private async Task<string> SendAsync(HttpRequestMessage request)
    {
        try
        {
            AppendAuthorization(request);
            using var client = _clientFactory.CreateClient(nameof(SandboxApiClient));
            var response = await client.SendAsync(request);
            var content = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return $"Sandbox request failed ({(int)response.StatusCode}): {Truncate(content)}";
            }

            return Truncate(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sandbox request error: {Message}", ex.Message);
            return $"Sandbox request error: {ex.Message}";
        }
    }

    private void AppendAuthorization(HttpRequestMessage request)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return;
        }

        var headerName = string.IsNullOrWhiteSpace(_settings.ApiKeyHeader) ? "Authorization" : _settings.ApiKeyHeader;
        if (request.Headers.Contains(headerName))
        {
            return;
        }

        var headerValue = headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
            ? $"Bearer {_settings.ApiKey}"
            : _settings.ApiKey;

        request.Headers.TryAddWithoutValidation(headerName, headerValue);
    }

    private Uri BuildUri(string path)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl) ? "http://localhost:3000" : _settings.BaseUrl;
        var normalizedBase = baseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith("/") ? path[1..] : path;
        return new Uri($"{normalizedBase}/{normalizedPath}");
    }

    private string Truncate(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        var maxLength = _settings.ResponseMaxLength > 0 ? _settings.ResponseMaxLength : content.Length;
        if (content.Length > maxLength)
        {
            return $"{content.Substring(0, maxLength)}...(truncated)";
        }

        return content;
    }
}
