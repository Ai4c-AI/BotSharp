using BotSharp.Abstraction.AIContext;
using BotSharp.Abstraction.Routing.Models;
using BotSharp.Abstraction.Templating;

namespace BotSharp.Core.Routing;

public partial class RoutingService
{
    public async Task<bool> InvokeAgent(
        string agentId,
        List<RoleDialogModel> dialogs,
        InvokeAgentOptions? options = null)
    {
        options ??= InvokeAgentOptions.Default();
        var agentService = _services.GetRequiredService<IAgentService>();
        var agent = await agentService.LoadAgent(agentId);

        Context.IncreaseRecursiveCounter();
        if (Context.CurrentRecursionDepth > agent.LlmConfig.MaxRecursionDepth)
        {
            _logger.LogWarning($"Current recursive call depth greater than {agent.LlmConfig.MaxRecursionDepth}, which will cause unexpected result.");
            return false;
        }

        var provider = agent.LlmConfig.Provider;
        var model = agent.LlmConfig.Model;

        if (provider == null || model == null)
        {
            var agentSettings = _services.GetRequiredService<AgentSettings>();
            provider = agentSettings.LlmConfig.Provider;
            model = agentSettings.LlmConfig.Model;
        }

        var chatCompletion = CompletionProvider.GetChatCompletion(_services, 
            provider: provider,
            model: model);

        // Get conversation ID
        var conv = _services.GetRequiredService<IConversationService>();
        var conversationId = conv.ConversationId;

        // Call AI Context Providers before invoking the model (InvokingAsync)
        var contextProviders = _services.GetServices<IAIContextProvider>()
            .OrderBy(p => p.Priority)
            .ToList();

        var invokingContext = new InvokingContext
        {
            Agent = agent,
            Dialogs = dialogs,
            ConversationId = conversationId
        };

        // Collect context from all providers
        var workingDialogs = new List<RoleDialogModel>(dialogs);
        foreach (var contextProvider in contextProviders)
        {
            try
            {
                var aiContext = await contextProvider.InvokingAsync(invokingContext);
                if (aiContext != null && aiContext.ContextMessages.Count > 0)
                {
                    // Insert context messages before the last user message
                    var lastIndex = workingDialogs.Count > 0 ? workingDialogs.Count - 1 : 0;
                    workingDialogs.InsertRange(lastIndex, aiContext.ContextMessages);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AI Context Provider {contextProvider.GetType().Name}.InvokingAsync");
            }
        }

        RoleDialogModel response;
        var message = dialogs.Last();
        if (options?.UseStream == true)
        {
            response = await chatCompletion.GetChatCompletionsStreamingAsync(agent, workingDialogs);
        }
        else
        {
            response = await chatCompletion.GetChatCompletions(agent, workingDialogs);
        }

        // Call AI Context Providers after the model has been invoked (InvokedAsync)
        var invokedContext = new InvokedContext
        {
            Agent = agent,
            RequestDialogs = workingDialogs,
            Response = response,
            ConversationId = conversationId
        };

        foreach (var contextProvider in contextProviders)
        {
            try
            {
                await contextProvider.InvokedAsync(invokedContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AI Context Provider {contextProvider.GetType().Name}.InvokedAsync");
            }
        }

        if (response.Role == AgentRole.Function)
        {
            message = RoleDialogModel.From(message, role: AgentRole.Function);
            if (response.FunctionName != null && response.FunctionName.Contains("/"))
            {
                response.FunctionName = response.FunctionName.Split("/").Last();
            }
            message.ToolCallId = response.ToolCallId;
            message.FunctionName = response.FunctionName;
            message.FunctionArgs = response.FunctionArgs;
            message.Indication = response.Indication;
            message.CurrentAgentId = agent.Id;
            message.IsStreaming = response.IsStreaming;
            message.MessageLabel = response.MessageLabel;

            await InvokeFunction(message, dialogs, options);
        }
        else
        {
            // Handle output routing exception.
            if (agent.Type == AgentType.Routing)
            {
                // Forgot about what situation needs to handle in this way
                response.Content = "Apologies, I'm not quite sure I understand. Could you please provide additional clarification or context?";
            }

            message = RoleDialogModel.From(message, role: AgentRole.Assistant, content: response.Content);
            message.CurrentAgentId = agent.Id;
            message.IsStreaming = response.IsStreaming;
            message.MessageLabel = response.MessageLabel;
            dialogs.Add(message);
            Context.AddDialogs([message]);
        }

        return true;
    }

    private async Task<bool> InvokeFunction(
        RoleDialogModel message,
        List<RoleDialogModel> dialogs,
        InvokeAgentOptions? options = null)
    {
        // execute function
        // Save states
        var states = _services.GetRequiredService<IConversationStateService>();
        states.SaveStateByArgs(message.FunctionArgs?.JsonContent<JsonDocument>());

        var routing = _services.GetRequiredService<IRoutingService>();
        // Call functions
        var funcOptions = options != null ? new InvokeFunctionOptions() { From = options.From } : null;
        await routing.InvokeFunction(message.FunctionName, message, options: funcOptions);

        // Pass execution result to LLM to get response
        if (!message.StopCompletion)
        {
            // Find response template
            var templateService = _services.GetRequiredService<IResponseTemplateService>();
            var responseTemplate = await templateService.RenderFunctionResponse(message.CurrentAgentId, message);
            if (!string.IsNullOrEmpty(responseTemplate))
            {
                var msg = RoleDialogModel.From(message,
                    role: AgentRole.Assistant,
                    content: responseTemplate);
                dialogs.Add(msg);
                Context.AddDialogs([msg]);
            }
            else
            {
                // Save to memory dialogs
                var msg = RoleDialogModel.From(message,
                    role: AgentRole.Function,
                    content: message.Content);

                dialogs.Add(msg);
                Context.AddDialogs([msg]);

                // Send to Next LLM
                var curAgentId = routing.Context.GetCurrentAgentId();
                await InvokeAgent(curAgentId, dialogs, options);
            }
        }
        else
        {
            var msg = RoleDialogModel.From(message,
                role: AgentRole.Assistant,
                content: message.Content);
            dialogs.Add(msg);
            Context.AddDialogs([msg]);
        }

        return true;
    }
}
