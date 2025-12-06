using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Plugin.MicrosoftAgentFramework.Functions;
using BotSharp.Plugin.MicrosoftAgentFramework.Models;
using BotSharp.Plugin.MicrosoftAgentFramework.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace BotSharp.Plugin.MicrosoftAgentFramework.Tests;

public class MafWorkflowExecutorFunctionTests
{
    private readonly Mock<ILogger<MafWorkflowExecutorFunction>> _loggerMock;
    private readonly MicrosoftAgentFrameworkSettings _settings;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly MafWorkflowExecutorFunction _function;

    public MafWorkflowExecutorFunctionTests()
    {
        _loggerMock = new Mock<ILogger<MafWorkflowExecutorFunction>>();
        _settings = new MicrosoftAgentFrameworkSettings
        {
            Enabled = true,
            EnableDetailedLogging = false,
            WorkflowTimeoutSeconds = 300
        };
        _serviceProviderMock = new Mock<IServiceProvider>();
        
        _function = new MafWorkflowExecutorFunction(
            _loggerMock.Object,
            _settings,
            _serviceProviderMock.Object
        );
    }

    [Fact]
    public void Function_Should_Have_Correct_Name()
    {
        // Assert
        _function.Name.ShouldBe("execute_maf_workflow");
    }

    [Fact]
    public async Task Execute_Should_Handle_IT_Support_Workflow()
    {
        // Arrange
        var request = new WorkflowExecutionRequest
        {
            WorkflowType = "it_support",
            InputData = new Dictionary<string, object>
            {
                ["issue_type"] = "Network Issue",
                ["description"] = "Cannot connect to VPN"
            }
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(request),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeTrue();
        message.Content.ShouldNotBeNullOrEmpty();
        message.Content.ShouldContain("IT Support");
        message.Content.ShouldContain("Network Issue");
    }

    [Fact]
    public async Task Execute_Should_Handle_HR_Service_Workflow()
    {
        // Arrange
        var request = new WorkflowExecutionRequest
        {
            WorkflowType = "hr_service",
            InputData = new Dictionary<string, object>
            {
                ["request_type"] = "Leave Request",
                ["employee_id"] = "EMP-123"
            }
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(request),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeTrue();
        message.Content.ShouldNotBeNullOrEmpty();
        message.Content.ShouldContain("HR Service");
        message.Content.ShouldContain("Leave Request");
    }

    [Fact]
    public async Task Execute_Should_Handle_Customer_Service_Workflow()
    {
        // Arrange
        var request = new WorkflowExecutionRequest
        {
            WorkflowType = "customer_service",
            InputData = new Dictionary<string, object>
            {
                ["service_type"] = "General Inquiry"
            }
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(request),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeTrue();
        message.Content.ShouldNotBeNullOrEmpty();
        message.Content.ShouldContain("Customer Service");
    }

    [Fact]
    public async Task Execute_Should_Return_False_For_Invalid_Workflow_Type()
    {
        // Arrange
        var request = new WorkflowExecutionRequest
        {
            WorkflowType = "unknown_workflow",
            InputData = new Dictionary<string, object>()
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(request),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeFalse();
        message.Content.ShouldContain("failed");
    }

    [Fact]
    public async Task Execute_Should_Use_Custom_Timeout_From_Request()
    {
        // Arrange
        var request = new WorkflowExecutionRequest
        {
            WorkflowType = "it_support",
            InputData = new Dictionary<string, object>
            {
                ["issue_type"] = "Test"
            },
            TimeoutSeconds = 60 // Custom timeout
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(request),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeTrue();
    }
}
