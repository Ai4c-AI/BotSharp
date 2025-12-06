using BotSharp.Abstraction.Functions;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Plugin.MicrosoftAgentFramework.Models;
using BotSharp.Plugin.MicrosoftAgentFramework.Settings;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace BotSharp.Plugin.MicrosoftAgentFramework.Functions;

/// <summary>
/// MAF Order Workflow Function - Example implementation demonstrating the integration
/// between BotSharp (Control Plane) and Microsoft Agent Framework (Execution Plane)
/// 
/// This function wraps a Microsoft Agent Framework workflow for order processing,
/// including validation, refunds, and cancellations with deterministic business logic.
/// </summary>
public class MafOrderWorkflowFunction : IFunctionCallback
{
    public string Name => "execute_order_workflow";
    
    public string Indication => "正在启动订单处理工作流... (Starting order processing workflow...)";

    private readonly ILogger<MafOrderWorkflowFunction> _logger;
    private readonly MicrosoftAgentFrameworkSettings _settings;
    private readonly IServiceProvider _services;

    public MafOrderWorkflowFunction(
        ILogger<MafOrderWorkflowFunction> logger,
        MicrosoftAgentFrameworkSettings settings,
        IServiceProvider services)
    {
        _logger = logger;
        _settings = settings;
        _services = services;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (_settings.EnableDetailedLogging)
            {
                _logger.LogInformation($"[MAF] Starting order workflow execution with args: {message.FunctionArgs}");
            }

            // 1. Parse input parameters from BotSharp
            var orderDetails = ParseOrderRequest(message.FunctionArgs);

            if (orderDetails == null)
            {
                message.Content = "Invalid order request format. Please provide valid order details.";
                return false;
            }

            // 2. Execute MAF workflow based on action type
            var result = await ExecuteMafWorkflow(orderDetails, message);

            // 3. Set the response content
            message.Content = result.Output;

            // 4. Store state if workflow is suspended (for human approval scenarios)
            if (!string.IsNullOrEmpty(result.SerializedState) && _settings.EnableStateSerialization)
            {
                // Store serialized state in conversation context for later resumption
                message.Payload = result.SerializedState;
            }

            stopwatch.Stop();
            _logger.LogInformation($"[MAF] Workflow execution completed in {stopwatch.ElapsedMilliseconds}ms. Success: {result.Success}");

            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MAF] Error executing order workflow");
            message.Content = $"Error processing order: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Parse order request from JSON input
    /// </summary>
    private OrderRequest? ParseOrderRequest(string jsonInput)
    {
        try
        {
            return JsonSerializer.Deserialize<OrderRequest>(jsonInput);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MAF] Failed to parse order request");
            return null;
        }
    }

    /// <summary>
    /// Execute MAF workflow - This is where the Microsoft Agent Framework integration happens
    /// 
    /// In a real implementation, this would:
    /// 1. Initialize MAF ChatClientAgents with OpenAI configuration
    /// 2. Build a workflow graph with agents for validation, processing, notification
    /// 3. Execute the workflow with deterministic control flow
    /// 4. Handle state serialization for long-running processes
    /// </summary>
    private async Task<WorkflowResult> ExecuteMafWorkflow(OrderRequest orderDetails, RoleDialogModel message)
    {
        var workflowId = Guid.NewGuid().ToString();
        var result = new WorkflowResult
        {
            WorkflowId = workflowId,
            Metadata = new Dictionary<string, object>
            {
                ["order_id"] = orderDetails.OrderId,
                ["action"] = orderDetails.Action,
                ["customer_id"] = orderDetails.CustomerId
            }
        };

        try
        {
            // Simulated MAF workflow execution
            // In production, this would use actual Microsoft.Agents.AI SDK:
            /*
            var validatorAgent = new ChatClientAgent(new OpenAIChatClient(...), "Validator");
            var processorAgent = new ChatClientAgent(new OpenAIChatClient(...), "Processor");
            var notificationAgent = new ChatClientAgent(new OpenAIChatClient(...), "Notifier");
            
            var workflow = new WorkflowBuilder(validatorAgent)
                .AddEdge(validatorAgent, processorAgent)
                .AddEdge(processorAgent, notificationAgent)
                .Build();
            
            var mafResult = await workflow.RunAsync(JsonSerializer.Serialize(orderDetails));
            */

            // Demonstration of deterministic business logic execution
            string workflowOutput = orderDetails.Action switch
            {
                "refund" => await ExecuteRefundWorkflow(orderDetails),
                "cancel" => await ExecuteCancelWorkflow(orderDetails),
                "process" => await ExecuteProcessWorkflow(orderDetails),
                _ => throw new InvalidOperationException($"Unknown action: {orderDetails.Action}")
            };

            result.Success = true;
            result.Output = workflowOutput;
            result.ExecutionTimeMs = 0; // Would be set by actual MAF execution
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[MAF] Workflow {workflowId} failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Output = $"Workflow execution failed: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Execute refund workflow with deterministic steps
    /// Demonstrates the MAF Sequential Workflow pattern from the problem statement
    /// </summary>
    private async Task<string> ExecuteRefundWorkflow(OrderRequest order)
    {
        var steps = new List<string>();

        // Step 1: Validate order
        steps.Add($"✓ Validated order {order.OrderId}");
        await Task.Delay(50); // Simulate processing

        // Step 2: Calculate refund amount
        steps.Add($"✓ Calculated refund amount: ¥{order.TotalAmount:F2}");
        await Task.Delay(50);

        // Step 3: Lock inventory
        steps.Add($"✓ Locked inventory for {order.Items.Count} items");
        await Task.Delay(50);

        // Step 4: Initiate refund
        steps.Add($"✓ Initiated refund transaction");
        await Task.Delay(50);

        // Step 5: Send notification
        steps.Add($"✓ Sent refund notification to customer {order.CustomerId}");

        var output = $@"订单退款工作流执行成功 (Order Refund Workflow Completed Successfully)

订单号: {order.OrderId}
客户ID: {order.CustomerId}
退款金额: ¥{order.TotalAmount:F2}
退款原因: {order.Reason ?? "客户要求"}

执行步骤:
{string.Join("\n", steps)}

状态: 已完成
备注: 退款将在3-5个工作日内到账";

        return output;
    }

    /// <summary>
    /// Execute cancellation workflow
    /// </summary>
    private async Task<string> ExecuteCancelWorkflow(OrderRequest order)
    {
        var steps = new List<string>();

        steps.Add($"✓ Validated order {order.OrderId} can be cancelled");
        await Task.Delay(50);

        steps.Add($"✓ Released inventory hold");
        await Task.Delay(50);

        steps.Add($"✓ Cancelled payment authorization");
        await Task.Delay(50);

        steps.Add($"✓ Sent cancellation notification to customer {order.CustomerId}");

        var output = $@"订单取消工作流执行成功 (Order Cancellation Workflow Completed Successfully)

订单号: {order.OrderId}
客户ID: {order.CustomerId}
取消原因: {order.Reason ?? "客户要求"}

执行步骤:
{string.Join("\n", steps)}

状态: 已取消";

        return output;
    }

    /// <summary>
    /// Execute order processing workflow
    /// </summary>
    private async Task<string> ExecuteProcessWorkflow(OrderRequest order)
    {
        var steps = new List<string>();

        steps.Add($"✓ Validated order details");
        await Task.Delay(50);

        steps.Add($"✓ Verified inventory for {order.Items.Count} items");
        await Task.Delay(50);

        steps.Add($"✓ Processed payment: ¥{order.TotalAmount:F2}");
        await Task.Delay(50);

        steps.Add($"✓ Created shipment request");
        await Task.Delay(50);

        steps.Add($"✓ Sent order confirmation to customer {order.CustomerId}");

        var output = $@"订单处理工作流执行成功 (Order Processing Workflow Completed Successfully)

订单号: {order.OrderId}
客户ID: {order.CustomerId}
订单金额: ¥{order.TotalAmount:F2}
商品数量: {order.Items.Sum(i => i.Quantity)} 件

执行步骤:
{string.Join("\n", steps)}

状态: 已确认
预计发货时间: {DateTime.Now.AddDays(1):yyyy-MM-dd}";

        return output;
    }
}
