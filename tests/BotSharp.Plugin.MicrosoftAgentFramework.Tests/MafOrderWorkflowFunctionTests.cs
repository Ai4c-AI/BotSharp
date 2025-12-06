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

public class MafOrderWorkflowFunctionTests
{
    private readonly Mock<ILogger<MafOrderWorkflowFunction>> _loggerMock;
    private readonly MicrosoftAgentFrameworkSettings _settings;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly MafOrderWorkflowFunction _function;

    public MafOrderWorkflowFunctionTests()
    {
        _loggerMock = new Mock<ILogger<MafOrderWorkflowFunction>>();
        _settings = new MicrosoftAgentFrameworkSettings
        {
            Enabled = true,
            EnableDetailedLogging = false,
            WorkflowTimeoutSeconds = 300
        };
        _serviceProviderMock = new Mock<IServiceProvider>();
        
        _function = new MafOrderWorkflowFunction(
            _loggerMock.Object,
            _settings,
            _serviceProviderMock.Object
        );
    }

    [Fact]
    public void Function_Should_Have_Correct_Name()
    {
        // Assert
        _function.Name.ShouldBe("execute_order_workflow");
    }

    [Fact]
    public void Function_Should_Have_Indication()
    {
        // Assert
        _function.Indication.ShouldNotBeNullOrEmpty();
        _function.Indication.ShouldContain("订单");
    }

    [Fact]
    public async Task Execute_Should_Handle_Refund_Request()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            OrderId = "ORD-12345",
            CustomerId = "CUST-67890",
            TotalAmount = 199.98m,
            Action = "refund",
            Reason = "Customer request",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = "PROD-001",
                    ProductName = "Test Product",
                    Quantity = 2,
                    Price = 99.99m
                }
            }
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(orderRequest),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeTrue();
        message.Content.ShouldNotBeNullOrEmpty();
        message.Content.ShouldContain("订单退款");
        message.Content.ShouldContain("ORD-12345");
        message.Content.ShouldContain("CUST-67890");
    }

    [Fact]
    public async Task Execute_Should_Handle_Cancel_Request()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            OrderId = "ORD-99999",
            CustomerId = "CUST-11111",
            TotalAmount = 299.99m,
            Action = "cancel",
            Reason = "Changed mind",
            Items = new List<OrderItem>()
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(orderRequest),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeTrue();
        message.Content.ShouldNotBeNullOrEmpty();
        message.Content.ShouldContain("取消");
        message.Content.ShouldContain("ORD-99999");
    }

    [Fact]
    public async Task Execute_Should_Handle_Process_Request()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            OrderId = "ORD-55555",
            CustomerId = "CUST-22222",
            TotalAmount = 499.99m,
            Action = "process",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = "PROD-002",
                    ProductName = "Another Product",
                    Quantity = 1,
                    Price = 499.99m
                }
            }
        };

        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = JsonSerializer.Serialize(orderRequest),
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeTrue();
        message.Content.ShouldNotBeNullOrEmpty();
        message.Content.ShouldContain("处理");
        message.Content.ShouldContain("已确认");
    }

    [Fact]
    public async Task Execute_Should_Return_False_For_Invalid_Json()
    {
        // Arrange
        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = "invalid json {{{",
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeFalse();
        message.Content.ShouldContain("Invalid");
    }

    [Fact]
    public async Task Execute_Should_Return_False_For_Null_Args()
    {
        // Arrange
        var message = new RoleDialogModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = "function",
            FunctionArgs = null,
            Content = string.Empty
        };

        // Act
        var result = await _function.Execute(message);

        // Assert
        result.ShouldBeFalse();
    }
}
