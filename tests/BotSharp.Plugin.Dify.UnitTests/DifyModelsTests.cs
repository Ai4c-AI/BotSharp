using BotSharp.Plugin.Dify.Models;
using Xunit;

namespace BotSharp.Plugin.Dify.UnitTests.Models;

public class DifyWorkflowModelsTests
{
    [Fact]
    public void DifyWorkflowRequest_ShouldInitializeWithDefaults()
    {
        // Act
        var request = new DifyWorkflowRequest();

        // Assert
        Assert.NotNull(request.Inputs);
        Assert.Empty(request.Inputs);
        Assert.Equal("blocking", request.ResponseMode);
        Assert.Equal("default-user", request.User);
    }

    [Fact]
    public void DifyWorkflowArgs_AsyncProperty_ShouldDefaultToFalse()
    {
        // Act
        var args = new DifyWorkflowArgs();

        // Assert
        Assert.False(args.Async);
    }

    [Fact]
    public void DifyWorkflowTask_ShouldInitializeWithCorrectDefaults()
    {
        // Act
        var task = new DifyWorkflowTask();

        // Assert
        Assert.Equal(string.Empty, task.TaskId);
        Assert.Equal(BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Running, task.Status);
        Assert.Equal(0, task.PollingAttempts);
        Assert.True(task.CreatedAt <= DateTime.UtcNow);
        Assert.True(task.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void DifyWorkflowResponse_ShouldHandleNullData()
    {
        // Act
        var response = new DifyWorkflowResponse
        {
            WorkflowRunId = "test-run-id",
            Status = "succeeded",
            Data = null
        };

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test-run-id", response.WorkflowRunId);
        Assert.Null(response.Data);
    }
}
