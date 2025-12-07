namespace BotSharp.Plugin.AgentProtocol.Functions;

/// <summary>
/// Function to invoke a remote A2A agent
/// </summary>
public class InvokeA2AAgentFn : IFunctionCallback
{
    public string Name => "a2a-invoke_remote_agent";
    public string Indication => "Connecting to remote agent...";

    private readonly ILogger<InvokeA2AAgentFn> _logger;
    private readonly IA2AClient _a2aClient;
    private readonly IConversationService _conversationService;
    private readonly IConversationStorage _conversationStorage;

    public InvokeA2AAgentFn(
        ILogger<InvokeA2AAgentFn> logger,
        IA2AClient a2aClient,
        IConversationService conversationService,
        IConversationStorage conversationStorage)
    {
        _logger = logger;
        _a2aClient = a2aClient;
        _conversationService = conversationService;
        _conversationStorage = conversationStorage;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        try
        {
            var args = JsonSerializer.Deserialize<A2AInvokeRequest>(message.FunctionArgs ?? "{}");
            
            if (args == null || string.IsNullOrEmpty(args.AgentId))
            {
                message.Content = "AgentId parameter is required";
                return false;
            }

            _logger.LogInformation($"Invoking remote A2A agent: {args.AgentId}");

            // Get conversation context if not provided
            if (!args.Context.Any())
            {
                var dialogs = _conversationStorage.GetDialogs(_conversationService.ConversationId);
                args.Context = dialogs;
            }

            var response = await _a2aClient.InvokeAgentAsync(args);

            if (response.Status == A2AResponseStatus.Success && response.Response != null)
            {
                message.Content = response.Response.Content;
                _logger.LogInformation($"Successfully invoked remote agent {args.AgentId}");
                return true;
            }
            else if (response.IsAsync)
            {
                message.Content = JsonSerializer.Serialize(new
                {
                    success = true,
                    async_operation = true,
                    request_id = response.RequestId,
                    message = "Request is being processed asynchronously. You can check the progress using the request ID."
                });
                _logger.LogInformation($"Remote agent {args.AgentId} processing asynchronously. RequestId: {response.RequestId}");
                return true;
            }
            else
            {
                message.Content = $"Failed to invoke remote agent: {response.ErrorMessage}";
                _logger.LogWarning($"Failed to invoke remote agent {args.AgentId}: {response.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking remote A2A agent");
            message.Content = $"Error invoking remote agent: {ex.Message}";
            return false;
        }
    }
}
