using Azure;
using BotSharp.Abstraction.Files.Utilities;
using BotSharp.Abstraction.Hooks;
using OpenAI.Chat;
using System.ClientModel;

namespace BotSharp.Plugin.AzureOpenAI.Providers.Chat;

public class ChatCompletionProvider : IChatCompletion
{
    protected readonly AzureOpenAiSettings _settings;
    protected readonly IServiceProvider _services;
    protected readonly ILogger<ChatCompletionProvider> _logger;
    private List<string> renderedInstructions = [];

    protected string _model;

    public virtual string Provider => "azure-openai";
    public string Model => _model;

    public ChatCompletionProvider(
        AzureOpenAiSettings settings,
        ILogger<ChatCompletionProvider> logger,
        IServiceProvider services)
    {
        _settings = settings;
        _logger = logger;
        _services = services;
    }

    public async Task<RoleDialogModel> GetChatCompletions(Agent agent, List<RoleDialogModel> conversations)
    {
        var contentHooks = _services.GetHooks<IContentGeneratingHook>(agent.Id);

        // Before chat completion hook
        foreach (var hook in contentHooks)
        {
            await hook.BeforeGenerating(agent, conversations);
        }

        var client = ProviderHelper.GetClient(Provider, _model, _services);
        var chatClient = client.GetChatClient(_model);
        var (prompt, messages, options) = PrepareOptions(agent, conversations);

        ClientResult<ChatCompletion>? response = null;
        ChatCompletion value = default;
        RoleDialogModel responseMessage;

        try
        {
            response = chatClient.CompleteChat(messages, options);
            value = response.Value;

            var reason = value.FinishReason;
            var content = value.Content;
            var text = content.FirstOrDefault()?.Text ?? string.Empty;

            if (reason == ChatFinishReason.FunctionCall || reason == ChatFinishReason.ToolCalls)
            {
                var toolCall = value.ToolCalls.FirstOrDefault();
                responseMessage = new RoleDialogModel(AgentRole.Function, text)
                {
                    CurrentAgentId = agent.Id,
                    MessageId = conversations.LastOrDefault()?.MessageId ?? string.Empty,
                    FunctionName = toolCall?.FunctionName,
                    FunctionArgs = toolCall?.FunctionArguments?.ToString(),
                    RenderedInstruction = string.Join("\r\n", renderedInstructions)
                };

                // Somethings LLM will generate a function name with agent name.
                if (!string.IsNullOrEmpty(responseMessage.FunctionName))
                {
                    responseMessage.FunctionName = responseMessage.FunctionName.Split('.').Last();
                }
            }
            else
            {
                responseMessage = new RoleDialogModel(AgentRole.Assistant, text)
                {
                    CurrentAgentId = agent.Id,
                    MessageId = conversations.LastOrDefault()?.MessageId ?? string.Empty,
                    RenderedInstruction = string.Join("\r\n", renderedInstructions)
                };
            }
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, ex.Message);
            responseMessage = new RoleDialogModel(AgentRole.Assistant, "The response was filtered due to the prompt triggering our content management policy. Please modify your prompt and retry.")
            {
                CurrentAgentId = agent.Id,
                MessageId = conversations.LastOrDefault()?.MessageId ?? string.Empty,
                RenderedInstruction = string.Join("\r\n", renderedInstructions)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            responseMessage = new RoleDialogModel(AgentRole.Assistant, ex.Message)
            {
                CurrentAgentId = agent.Id,
                MessageId = conversations.LastOrDefault()?.MessageId ?? string.Empty,
                RenderedInstruction = string.Join("\r\n", renderedInstructions)
            };
        }

        var tokenUsage = response?.Value?.Usage;
        var inputTokenDetails = response?.Value?.Usage?.InputTokenDetails;

        // After chat completion hook
        foreach (var hook in contentHooks)
        {
            await hook.AfterGenerated(responseMessage, new TokenStatsModel
            {
                Prompt = prompt,
                Provider = Provider,
                Model = _model,
                TextInputTokens = (tokenUsage?.InputTokenCount ?? 0) - (inputTokenDetails?.CachedTokenCount ?? 0),
                CachedTextInputTokens = inputTokenDetails?.CachedTokenCount ?? 0,
                TextOutputTokens = tokenUsage?.OutputTokenCount ?? 0
            });
        }

        return responseMessage;
    }

    public async Task<bool> GetChatCompletionsAsync(Agent agent,
        List<RoleDialogModel> conversations,
        Func<RoleDialogModel, Task> onMessageReceived,
        Func<RoleDialogModel, Task> onFunctionExecuting)
    {
        var hooks = _services.GetHooks<IContentGeneratingHook>(agent.Id);

        // Before chat completion hook
        foreach (var hook in hooks)
        {
            await hook.BeforeGenerating(agent, conversations);
        }

        var client = ProviderHelper.GetClient(Provider, _model, _services);
        var chatClient = client.GetChatClient(_model);
        var (prompt, messages, options) = PrepareOptions(agent, conversations);

        var response = await chatClient.CompleteChatAsync(messages, options);
        var value = response.Value;
        var reason = value.FinishReason;
        var content = value.Content;
        var text = content.FirstOrDefault()?.Text ?? string.Empty;

        var msg = new RoleDialogModel(AgentRole.Assistant, text)
        {
            CurrentAgentId = agent.Id,
            RenderedInstruction = string.Join("\r\n", renderedInstructions)
        };

        var tokenUsage = response?.Value?.Usage;
        var inputTokenDetails = response?.Value?.Usage?.InputTokenDetails;

        // After chat completion hook
        foreach (var hook in hooks)
        {
            await hook.AfterGenerated(msg, new TokenStatsModel
            {
                Prompt = prompt,
                Provider = Provider,
                Model = _model,
                TextInputTokens = (tokenUsage?.InputTokenCount ?? 0) - (inputTokenDetails?.CachedTokenCount ?? 0),
                CachedTextInputTokens = inputTokenDetails?.CachedTokenCount ?? 0,
                TextOutputTokens = tokenUsage?.OutputTokenCount ?? 0
            });
        }

        if (reason == ChatFinishReason.FunctionCall || reason == ChatFinishReason.ToolCalls)
        {
            var toolCall = value.ToolCalls?.FirstOrDefault();
            _logger.LogInformation($"[{agent.Name}]: {toolCall?.FunctionName}({toolCall?.FunctionArguments})");

            var funcContextIn = new RoleDialogModel(AgentRole.Function, text)
            {
                CurrentAgentId = agent.Id,
                MessageId = conversations.LastOrDefault()?.MessageId ?? string.Empty,
                ToolCallId = toolCall?.Id,
                FunctionName = toolCall?.FunctionName,
                FunctionArgs = toolCall?.FunctionArguments?.ToString(),
                RenderedInstruction = string.Join("\r\n", renderedInstructions)
            };

            // Somethings LLM will generate a function name with agent name.
            if (!string.IsNullOrEmpty(funcContextIn.FunctionName))
            {
                funcContextIn.FunctionName = funcContextIn.FunctionName.Split('.').Last();
            }

            // Execute functions
            await onFunctionExecuting(funcContextIn);
        }
        else
        {
            // Text response received
            await onMessageReceived(msg);
        }

        return true;
    }

    public async Task<bool> GetChatCompletionsStreamingAsync(Agent agent, List<RoleDialogModel> conversations, Func<RoleDialogModel, Task> onMessageReceived)
    {
        var client = ProviderHelper.GetClient(Provider, _model, _services);
        var chatClient = client.GetChatClient(_model);
        var (prompt, messages, options) = PrepareOptions(agent, conversations);

        var response = chatClient.CompleteChatStreamingAsync(messages, options);

        await foreach (var choice in response)
        {
            if (choice.FinishReason == ChatFinishReason.FunctionCall || choice.FinishReason == ChatFinishReason.ToolCalls)
            {
                var update = choice.ToolCallUpdates?.FirstOrDefault()?.FunctionArgumentsUpdate?.ToString() ?? string.Empty;
                Console.Write(update);

                await onMessageReceived(new RoleDialogModel(AgentRole.Assistant, update)
                {
                    RenderedInstruction = string.Join("\r\n", renderedInstructions)
                });
                continue;
            }

            if (choice.ContentUpdate.IsNullOrEmpty()) continue;

            _logger.LogInformation(choice.ContentUpdate[0]?.Text);

            await onMessageReceived(new RoleDialogModel(choice.Role?.ToString() ?? ChatMessageRole.Assistant.ToString(), choice.ContentUpdate[0]?.Text ?? string.Empty)
            {
                RenderedInstruction = string.Join("\r\n", renderedInstructions)
            });
        }

        return true;
    }

    protected (string, IEnumerable<ChatMessage>, ChatCompletionOptions) PrepareOptions(Agent agent, List<RoleDialogModel> conversations)
    {
        var agentService = _services.GetRequiredService<IAgentService>();
        var state = _services.GetRequiredService<IConversationStateService>();
        var fileStorage = _services.GetRequiredService<IFileStorageService>();
        var settingsService = _services.GetRequiredService<ILlmProviderService>();
        var settings = settingsService.GetSetting(Provider, _model);
        var allowMultiModal = settings != null && settings.MultiModal;
        renderedInstructions = [];

        var messages = new List<ChatMessage>();

        var temperature = float.Parse(state.GetState("temperature", "0.0"));
        var maxTokens = int.TryParse(state.GetState("max_tokens"), out var tokens)
                            ? tokens
                            : agent.LlmConfig?.MaxOutputTokens ?? LlmConstant.DEFAULT_MAX_OUTPUT_TOKEN;

        var options = new ChatCompletionOptions()
        {
            Temperature = temperature,
            MaxOutputTokenCount = maxTokens
        };

        var functions = agent.Functions.Concat(agent.SecondaryFunctions ?? []);
        foreach (var function in functions)
        {
            if (!agentService.RenderFunction(agent, function)) continue;

            var property = agentService.RenderFunctionProperty(agent, function);

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: function.Name,
                functionDescription: function.Description,
                functionParameters: BinaryData.FromObjectAsJson(property)));
        }

        if (!string.IsNullOrEmpty(agent.Instruction) || !agent.SecondaryInstructions.IsNullOrEmpty())
        {
            var instruction = agentService.RenderedInstruction(agent);
            renderedInstructions.Add(instruction);
            messages.Add(new SystemChatMessage(instruction));
        }

        if (!string.IsNullOrEmpty(agent.Knowledges))
        {
            messages.Add(new SystemChatMessage(agent.Knowledges));
        }

        var samples = ProviderHelper.GetChatSamples(agent.Samples);
        foreach (var sample in samples)
        {
            messages.Add(sample.Role == AgentRole.User ? new UserChatMessage(sample.Content) : new AssistantChatMessage(sample.Content));
        }

        var filteredMessages = conversations.Select(x => x).ToList();
        var firstUserMsgIdx = filteredMessages.FindIndex(x => x.Role == AgentRole.User);
        if (firstUserMsgIdx > 0)
        {
            filteredMessages = filteredMessages.Where((_, idx) => idx >= firstUserMsgIdx).ToList();
        }

        foreach (var message in filteredMessages)
        {
            if (message.Role == AgentRole.Function)
            {
                messages.Add(new AssistantChatMessage(new List<ChatToolCall>
                {
                    ChatToolCall.CreateFunctionToolCall(message.ToolCallId.IfNullOrEmptyAs(message.FunctionName), message.FunctionName, BinaryData.FromString(message.FunctionArgs ?? "{}"))
                }));

                messages.Add(new ToolChatMessage(message.ToolCallId.IfNullOrEmptyAs(message.FunctionName), message.Content));
            }
            else if (message.Role == AgentRole.User)
            {
                var text = !string.IsNullOrWhiteSpace(message.Payload) ? message.Payload : message.Content;
                var textPart = ChatMessageContentPart.CreateTextPart(text);
                var contentParts = new List<ChatMessageContentPart> { textPart };

                if (allowMultiModal && !message.Files.IsNullOrEmpty())
                {
                    foreach (var file in message.Files)
                    {
                        if (!string.IsNullOrEmpty(file.FileData))
                        {
                            var (contentType, binary) = FileUtility.GetFileInfoFromData(file.FileData);
                            var contentPart = ChatMessageContentPart.CreateImagePart(binary, contentType.IfNullOrEmptyAs(file.ContentType), ChatImageDetailLevel.Auto);
                            contentParts.Add(contentPart);
                        }
                        else if (!string.IsNullOrEmpty(file.FileStorageUrl))
                        {
                            var contentType = FileUtility.GetFileContentType(file.FileStorageUrl);
                            var binary = fileStorage.GetFileBytes(file.FileStorageUrl);
                            var contentPart = ChatMessageContentPart.CreateImagePart(binary, contentType.IfNullOrEmptyAs(file.ContentType), ChatImageDetailLevel.Auto);
                            contentParts.Add(contentPart);
                        }
                        else if (!string.IsNullOrEmpty(file.FileUrl))
                        {
                            var uri = new Uri(file.FileUrl);
                            var contentPart = ChatMessageContentPart.CreateImagePart(uri, ChatImageDetailLevel.Auto);
                            contentParts.Add(contentPart);
                        }
                    }
                }
                messages.Add(new UserChatMessage(contentParts) { ParticipantName = message.FunctionName });
            }
            else if (message.Role == AgentRole.Assistant)
            {
                messages.Add(new AssistantChatMessage(message.Content));
            }
        }

        var prompt = GetPrompt(messages, options);
        return (prompt, messages, options);
    }

    private string GetPrompt(IEnumerable<ChatMessage> messages, ChatCompletionOptions options)
    {
        var prompt = string.Empty;

        if (!messages.IsNullOrEmpty())
        {
            // System instruction
            var verbose = string.Join("\r\n", messages
                .Select(x => x as SystemChatMessage)
                .Where(x => x != null)
                .Select(x =>
                {
                    if (!string.IsNullOrEmpty(x.ParticipantName))
                    {
                        // To display Agent name in log
                        return $"[{x.ParticipantName}]: {x.Content.FirstOrDefault()?.Text ?? string.Empty}";
                    }
                    return $"{AgentRole.System}: {x.Content.FirstOrDefault()?.Text ?? string.Empty}";
                }));
            prompt += $"{verbose}\r\n";

            prompt += "\r\n[CONVERSATION]";
            verbose = string.Join("\r\n", messages
                .Where(x => x as SystemChatMessage == null)
                .Select(x =>
                {
                    var fnMessage = x as ToolChatMessage;
                    if (fnMessage != null)
                    {
                        return $"{AgentRole.Function}: {fnMessage.Content.FirstOrDefault()?.Text ?? string.Empty}";
                    }

                    var userMessage = x as UserChatMessage;
                    if (userMessage != null)
                    {
                        var content = x.Content.FirstOrDefault()?.Text ?? string.Empty;
                        return !string.IsNullOrEmpty(userMessage.ParticipantName) && userMessage.ParticipantName != "route_to_agent" ?
                            $"{userMessage.ParticipantName}: {content}" :
                            $"{AgentRole.User}: {content}";
                    }

                    var assistMessage = x as AssistantChatMessage;
                    if (assistMessage != null)
                    {
                        var toolCall = assistMessage.ToolCalls?.FirstOrDefault();
                        return toolCall != null ?
                            $"{AgentRole.Assistant}: Call function {toolCall?.FunctionName}({toolCall?.FunctionArguments})" :
                            $"{AgentRole.Assistant}: {assistMessage.Content.FirstOrDefault()?.Text ?? string.Empty}";
                    }

                    return string.Empty;
                }));
            prompt += $"\r\n{verbose}\r\n";
        }

        if (!options.Tools.IsNullOrEmpty())
        {
            var functions = string.Join("\r\n", options.Tools.Select(fn =>
            {
                return $"\r\n{fn.FunctionName}: {fn.FunctionDescription}\r\n{fn.FunctionParameters}";
            }));
            prompt += $"\r\n[FUNCTIONS]{functions}\r\n";
        }

        return prompt;
    }

    public void SetModelName(string model)
    {
        _model = model;
    }
}
