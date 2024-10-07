using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Files;
using BotSharp.Abstraction.Files.Utilities;
using BotSharp.Abstraction.Utilities;
using OpenAI.Chat;
using System.Text.Json;

namespace BotSharp.Plugin.OneAPI.Providers;

public abstract class OneAPIChatCompletionProvider : IChatCompletion
{
    public abstract string Provider { get; }

    private readonly OneAPISettings _settings;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private string _model;

    /// <summary>
    /// Creates a new OneAPI chat completion service.
    /// </summary>
    /// <param name="modelId"></param>
    /// <param name="dashScopeClient"></param>
    /// <param name="loggerFactory"></param>
    public OneAPIChatCompletionProvider(
        IServiceProvider services,
        OneAPISettings settings, 
        ILoggerFactory? loggerFactory = null)
    {
        _services = services;
        _settings = settings; 
        _logger = loggerFactory != null
            ? loggerFactory.CreateLogger<OneAPIChatCompletionProvider>()
            : NullLogger.Instance; 
    }

    public void SetModelName(string model)
    {
        _model = model;
    }

    public async Task<RoleDialogModel> GetChatCompletions(Agent agent, List<RoleDialogModel> conversations)
    {
        var contentHooks = _services.GetServices<IContentGeneratingHook>().ToList();

        // Before chat completion hook
        foreach (var hook in contentHooks)
        {
            await hook.BeforeGenerating(agent, conversations);
        }
        var settingsService = _services.GetRequiredService<ILlmProviderService>();
        var settings = settingsService.GetSetting(Provider, _model);

        var client = ProviderHelper.GetClient(settings);

        var (prompt, messages, options) = PrepareOptions(agent, conversations);
        var state = _services.GetRequiredService<IConversationStateService>();

        var temperature = float.Parse(state.GetState("temperature", "0.0"));
        var maxtoken = int.Parse(state.GetState("max_tokens", "1500"));
 
        var response = await client.CompleteChatAsync(
            messages, options);

        var chatCompletion = response.Value;

        RoleDialogModel responseMessage = null;
        switch (chatCompletion.FinishReason)
        {
            case ChatFinishReason.Stop:
                {
                    responseMessage = new RoleDialogModel(AgentRole.Assistant, chatCompletion.Content[0].Text)
                    {
                        CurrentAgentId = agent.Id,
                        MessageId = conversations.Last().MessageId
                    };
                    break;
                }

            case ChatFinishReason.ToolCalls:
                {
                    var toolcall = chatCompletion.ToolCalls.FirstOrDefault();

                    responseMessage = new RoleDialogModel(AgentRole.Function, JsonSerializer.Serialize(toolcall))
                    {
                        CurrentAgentId = agent.Id,
                        MessageId = conversations.Last().MessageId,
                        FunctionName = toolcall?.FunctionName,
                        FunctionArgs = toolcall?.FunctionArguments.ToString(),
                    }; 
                    break;
                }

            case ChatFinishReason.Length:
                throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

            case ChatFinishReason.ContentFilter:
                throw new NotImplementedException("Omitted content due to a content filter flag.");

            case ChatFinishReason.FunctionCall:
                throw new NotImplementedException("Deprecated in favor of tool calls.");

            default:
                throw new NotImplementedException(chatCompletion.FinishReason.ToString());
        }        
       
        // After chat completion hook
        foreach (var hook in contentHooks)
        {
            await hook.AfterGenerated(responseMessage, new TokenStatsModel
            {
                Prompt = prompt,
                Provider = Provider,
                Model = _model,
                PromptCount = chatCompletion.Usage.OutputTokenCount,
                CompletionCount = chatCompletion.Usage.TotalTokenCount
            });
        }

        return responseMessage;
    }

    private (string, ChatMessage[], ChatCompletionOptions) PrepareOptions(Agent agent, List<RoleDialogModel> conversations)
    {
        var agentService = _services.GetRequiredService<IAgentService>();
        var fileService = _services.GetRequiredService<IFileStorageService>();
        var state = _services.GetRequiredService<IConversationStateService>();
        var settingsService = _services.GetRequiredService<ILlmProviderService>();
        var settings = settingsService.GetSetting(Provider, _model);
        var allowMultiModal = settings != null && settings.MultiModal;
        var messages = new List<ChatMessage>();

        var temperature = float.Parse(state.GetState("temperature", "0.0"));
        var maxTokens = int.Parse(state.GetState("max_tokens", "1024"));
        var options = new ChatCompletionOptions()
        {
            Temperature = temperature,
             MaxOutputTokenCount = maxTokens
        };

        if (!string.IsNullOrEmpty(agent.Instruction))
        {
            var instruction = agentService.RenderedInstruction(agent);
            messages.Add(new SystemChatMessage(instruction));
        }  

        foreach (var function in agent.Functions)
        {
            if (!agentService.RenderFunction(agent, function)) 
                continue;

            var property = agentService.RenderFunctionProperty(agent, function);
            options.Tools.Add(ChatTool.CreateFunctionTool(
               functionName: function.Name,
               functionDescription: function.Description,
               functionParameters: BinaryData.FromObjectAsJson(property)));

        }

        foreach (var message in conversations)
        {
            if (message.Role == AgentRole.Function)
            {
                FunctionRoleMessages(messages, message);
            }
            else if (message.Role == "user")
            {
                UserRoleMessages(fileService, allowMultiModal, messages, message);
            }
            else if (message.Role == "assistant")
            {
                messages.Add(new AssistantChatMessage(message.Content));
            }
        }

        var prompt = GetPrompt(messages, options);
        return (prompt, messages.ToArray(), options);
    }

    protected virtual void UserRoleMessages(IFileStorageService fileStorage, bool allowMultiModal, List<ChatMessage> messages, RoleDialogModel message)
    {
        var text = !string.IsNullOrWhiteSpace(message.Payload) ? message.Payload : message.Content;
        var textPart = ChatMessageContentPart.CreateTextPart(text);
        var contentParts = new List<ChatMessageContentPart> { textPart };

        if (allowMultiModal && !message.Files.IsNullOrEmpty())
        {
            foreach (var file in message.Files)
            {
                if (!string.IsNullOrEmpty(file.FileUrl))
                {
                    var uri = new Uri(file.FileUrl);
                    var contentPart = ChatMessageContentPart.CreateImagePart(uri, ChatImageDetailLevel.Low);
                    contentParts.Add(contentPart);
                }
                else if (!string.IsNullOrEmpty(file.FileData))
                {
                    var contentType = FileUtility.GetFileContentType(file.FileStorageUrl);
                    var bytes = fileStorage.GetFileBytes(file.FileStorageUrl);
                    var contentPart = ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(bytes), contentType,  ChatImageDetailLevel.Low);
                    contentParts.Add(contentPart);
                }
                else if (!string.IsNullOrEmpty(file.FileStorageUrl))
                {
                    var contentType = FileUtility.GetFileContentType(file.FileStorageUrl);
                    using var stream = File.OpenRead(file.FileStorageUrl);
                    var contentPart = ChatMessageContentPart.CreateImagePart(BinaryData.FromStream(stream), contentType, ChatImageDetailLevel.Low);
                    contentParts.Add(contentPart);
                }
            }
        }
        messages.Add(new UserChatMessage(contentParts) { ParticipantName = message.FunctionName });
    }

    protected virtual void FunctionRoleMessages(List<ChatMessage> messages, RoleDialogModel message)
    {
        //messages.Add(new AssistantChatMessage(message.Content)
        //{
        //    FunctionCall = new ChatFunctionCall(message.FunctionName, message.FunctionArgs ?? string.Empty)
        //});

        //messages.Add(new FunctionChatMessage(message.FunctionName, message.Content));
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
                    var fnMessage = x as FunctionChatMessage;
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
                        return assistMessage.FunctionCall != null ?
                            $"{AgentRole.Assistant}: Call function {assistMessage.FunctionCall.FunctionName}({assistMessage.FunctionCall.FunctionArguments})" :
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

    public async Task<bool> GetChatCompletionsAsync(Agent agent, List<RoleDialogModel> conversations, Func<RoleDialogModel, Task> onMessageReceived, Func<RoleDialogModel, Task> onFunctionExecuting)
    {
        var hooks = _services.GetServices<IContentGeneratingHook>().ToList();

        // Before chat completion hook
        foreach (var hook in hooks)
        {
            await hook.BeforeGenerating(agent, conversations);
        }
        var settingsService = _services.GetRequiredService<ILlmProviderService>();
        var settings = settingsService.GetSetting(Provider, _model);

        var client = ProviderHelper.GetClient(settings);
        var (prompt, messages, options) = PrepareOptions(agent, conversations);

        var response = await client.CompleteChatAsync(messages, options);
        var value = response.Value;
        var reason = value.FinishReason;
        var content = value.Content;
        var text = content.FirstOrDefault()?.Text ?? string.Empty;

        var msg = new RoleDialogModel(AgentRole.Assistant, text)
        {
            CurrentAgentId = agent.Id
        };

        // After chat completion hook
        foreach (var hook in hooks)
        {
            await hook.AfterGenerated(msg, new TokenStatsModel
            {
                Prompt = prompt,
                Provider = Provider,
                Model = _model,
                PromptCount = response.Value.Usage.InputTokenCount,
                CompletionCount = response.Value.Usage.OutputTokenCount
            });
        }

        if (reason == ChatFinishReason.FunctionCall)
        {
            _logger.LogInformation($"[{agent.Name}]: {value.FunctionCall.FunctionName}({value.FunctionCall.FunctionArguments})");

            var funcContextIn = new RoleDialogModel(AgentRole.Function, text)
            {
                CurrentAgentId = agent.Id,
                FunctionName = value.FunctionCall?.FunctionName,
                FunctionArgs = value.FunctionCall?.FunctionArguments.ToString()
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
        var settingsService = _services.GetRequiredService<ILlmProviderService>();
        var settings = settingsService.GetSetting(Provider, _model);

        var client = ProviderHelper.GetClient(settings);
        var (prompt, messages, options) = PrepareOptions(agent, conversations);

        var response = client.CompleteChatStreamingAsync(messages, options);

        await foreach (var choice in response)
        {
            if (choice.FinishReason == ChatFinishReason.FunctionCall)
            {
                Console.Write(choice.FunctionCallUpdate?.FunctionArgumentsUpdate);

                await onMessageReceived(new RoleDialogModel(AgentRole.Assistant, choice.FunctionCallUpdate?.FunctionArgumentsUpdate.ToString()));
                continue;
            }

            if (choice.ContentUpdate.IsNullOrEmpty()) continue;

            _logger.LogInformation(choice.ContentUpdate[0]?.Text);

            await onMessageReceived(new RoleDialogModel(choice.Role.ToString(), choice.ContentUpdate[0]?.Text ?? string.Empty));
        }

        return true;
    }
}
