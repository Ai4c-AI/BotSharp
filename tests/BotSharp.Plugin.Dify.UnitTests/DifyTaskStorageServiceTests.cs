using BotSharp.Plugin.Dify.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BotSharp.Plugin.Dify.UnitTests.Services;

public class DifyTaskStorageServiceTests
{
    private readonly Mock<ILogger<DifyTaskStorageService>> _loggerMock;
    private readonly DifyTaskStorageService _service;

    public DifyTaskStorageServiceTests()
    {
        _loggerMock = new Mock<ILogger<DifyTaskStorageService>>();
        _service = new DifyTaskStorageService(_loggerMock.Object);
    }

    [Fact]
    public void StoreTask_ShouldAddTaskToStorage()
    {
        // Arrange
        var task = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "test-task-1",
            WorkflowRunId = "workflow-run-1",
            WorkflowId = "workflow-1",
            Status = BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Running
        };

        // Act
        _service.StoreTask(task);
        var retrieved = _service.GetTask("test-task-1");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("test-task-1", retrieved.TaskId);
        Assert.Equal("workflow-run-1", retrieved.WorkflowRunId);
    }

    [Fact]
    public void GetTask_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = _service.GetTask("non-existent-task");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UpdateTask_ShouldModifyExistingTask()
    {
        // Arrange
        var task = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "test-task-2",
            WorkflowRunId = "workflow-run-2",
            Status = BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Running
        };
        _service.StoreTask(task);

        // Act
        task.Status = BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Succeeded;
        task.ResultData = "test result";
        _service.UpdateTask(task);

        var retrieved = _service.GetTask("test-task-2");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Succeeded, retrieved.Status);
        Assert.Equal("test result", retrieved.ResultData);
    }

    [Fact]
    public void GetRunningTasks_ShouldReturnOnlyRunningTasks()
    {
        // Arrange
        var runningTask = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "running-task",
            Status = BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Running
        };
        var completedTask = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "completed-task",
            Status = BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Succeeded
        };
        var failedTask = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "failed-task",
            Status = BotSharp.Plugin.Dify.Enums.DifyWorkflowStatus.Failed
        };

        _service.StoreTask(runningTask);
        _service.StoreTask(completedTask);
        _service.StoreTask(failedTask);

        // Act
        var runningTasks = _service.GetRunningTasks();

        // Assert
        Assert.Single(runningTasks);
        Assert.Equal("running-task", runningTasks[0].TaskId);
    }

    [Fact]
    public void RemoveTask_ShouldDeleteTaskFromStorage()
    {
        // Arrange
        var task = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "task-to-remove"
        };
        _service.StoreTask(task);

        // Act
        _service.RemoveTask("task-to-remove");
        var retrieved = _service.GetTask("task-to-remove");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetTasksByConversation_ShouldReturnTasksForSpecificConversation()
    {
        // Arrange
        var task1 = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "task-1",
            ConversationId = "conversation-1"
        };
        var task2 = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "task-2",
            ConversationId = "conversation-1"
        };
        var task3 = new BotSharp.Plugin.Dify.Models.DifyWorkflowTask
        {
            TaskId = "task-3",
            ConversationId = "conversation-2"
        };

        _service.StoreTask(task1);
        _service.StoreTask(task2);
        _service.StoreTask(task3);

        // Act
        var tasks = _service.GetTasksByConversation("conversation-1");

        // Assert
        Assert.Equal(2, tasks.Count);
        Assert.All(tasks, t => Assert.Equal("conversation-1", t.ConversationId));
    }
}
