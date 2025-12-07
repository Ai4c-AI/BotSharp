# BotSharp 代理协议 (A2A) 插件

## 概述

代理协议插件使 BotSharp 能够作为 **代理到代理 (A2A) 客户端**，通过标准化协议与远程代理进行通信。该插件实现了以下核心要求：

1. **A2A 客户端通信**：路由代理可以通过 A2AClient 调用远程代理，而不是直接调用 HTTP 接口
2. **语义路由**：通过代理注册中心进行动态代理发现，支持基于能力的匹配
3. **双向通信**：支持异步回调和推送通知，用于处理长时间运行的任务

## 功能特性

### 1. A2A 客户端

A2A 客户端 (`IA2AClient`) 使 BotSharp 能够与远程代理通信：

- **代理名片解析**：自动从远程端点获取并解析代理能力
- **智能调用**：向远程代理发送包含完整对话上下文的请求
- **异步支持**：通过回调机制处理长时间运行的操作
- **进度跟踪**：监控远程代理操作的进度

### 2. 代理注册中心

代理注册中心 (`IAgentRegistry`) 提供语义路由能力：

- **语义查询**：使用自然语言查询代理（例如："谁擅长处理税务计算？"）
- **能力匹配**：根据所需能力查找代理
- **置信度评分**：根据查询的相关性对代理进行排名
- **本地和远程**：支持本地代理缓存和远程注册中心端点

### 3. 双向通信

支持推送通知和异步回调：

- **异步回调**：远程代理可以异步发送响应
- **进度通知**：在长时间运行的操作期间接收进度更新
- **推送通知**：远程代理可以主动向 BotSharp 发送更新

## 架构

### 组件

```
BotSharp.Plugin.AgentProtocol/
├── Services/
│   ├── IA2AClient.cs               # A2A 客户端接口
│   ├── A2AClient.cs                # A2A 客户端实现
│   ├── IA2ACardResolver.cs         # 名片解析接口
│   ├── A2ACardResolver.cs          # 解析代理能力
│   ├── IAgentRegistry.cs           # 代理注册中心接口
│   └── AgentRegistry.cs            # 语义路由实现
├── Models/
│   ├── A2AAgentCard.cs             # 代理能力名片模型
│   ├── A2AInvokeModels.cs          # 请求/响应模型
│   └── AgentRegistryModels.cs      # 注册中心查询模型
├── Hooks/
│   └── A2AAgentHook.cs             # 与路由系统集成
├── Functions/
│   ├── QueryAgentRegistryFn.cs     # 语义代理搜索功能
│   └── InvokeA2AAgentFn.cs         # 远程代理调用功能
├── Settings/
│   └── AgentProtocolSettings.cs    # 插件配置
└── AgentProtocolPlugin.cs          # 插件注册
```

## 配置

在 `appsettings.json` 中添加以下配置：

```json
{
  "AgentProtocol": {
    "EnableA2AClient": true,
    "RegistryEndpoint": "https://your-registry-endpoint/api",
    "TimeoutSeconds": 300,
    "EnableAsyncCallbacks": true,
    "CallbackEndpoint": "https://your-botsharp-instance/api/a2a/callback",
    "EnableProgressNotifications": true,
    "RemoteAgents": {
      "tax-agent-id": "https://tax-agent.example.com",
      "data-analysis-agent-id": "https://analytics.example.com"
    }
  }
}
```

### 配置选项

| 选项 | 类型 | 默认值 | 描述 |
|-----|------|--------|------|
| `EnableA2AClient` | bool | true | 启用 A2A 客户端功能 |
| `RegistryEndpoint` | string | null | 用于语义路由的代理注册中心端点 URL |
| `TimeoutSeconds` | int | 300 | A2A 操作超时时间（秒） |
| `EnableAsyncCallbacks` | bool | true | 启用来自远程代理的异步回调 |
| `CallbackEndpoint` | string | null | 接收推送通知的回调端点 URL |
| `EnableProgressNotifications` | bool | true | 启用长时间运行任务的进度通知 |
| `RemoteAgents` | dictionary | {} | 代理 ID 到其端点 URL 的映射 |

## 使用方法

### 1. 查询代理注册中心

使用语义查询查找有能力的代理：

```csharp
var registry = serviceProvider.GetRequiredService<IAgentRegistry>();

var query = new AgentRegistryQuery
{
    Query = "谁擅长处理税务计算？",
    RequiredCapabilities = new List<string> { "tax-calculation" },
    MaxResults = 5
};

var result = await registry.QueryAgentsAsync(query);

foreach (var agent in result.Agents)
{
    var confidence = result.ConfidenceScores[agent.Id];
    Console.WriteLine($"代理: {agent.Name}, 置信度: {confidence:P}");
}
```

### 2. 调用远程代理

使用对话上下文调用远程代理：

```csharp
var a2aClient = serviceProvider.GetRequiredService<IA2AClient>();

var request = new A2AInvokeRequest
{
    AgentId = "tax-agent-id",
    Parameters = new Dictionary<string, object>
    {
        ["income"] = 50000,
        ["deductions"] = 10000
    },
    Context = dialogs // 当前对话上下文
};

var response = await a2aClient.InvokeAgentAsync(request);

if (response.Status == A2AResponseStatus.Success)
{
    Console.WriteLine($"响应: {response.Response.Content}");
}
else if (response.IsAsync)
{
    Console.WriteLine($"异步处理中。请求ID: {response.RequestId}");
}
```

### 3. 处理异步回调

为异步操作注册回调：

```csharp
// 注册完成回调
a2aClient.RegisterCallback(requestId, (response) =>
{
    Console.WriteLine($"异步操作完成: {response.Response.Content}");
});

// 注册进度回调
a2aClient.RegisterProgressCallback(requestId, (notification) =>
{
    Console.WriteLine($"进度: {notification.Progress}% - {notification.Message}");
    
    if (notification.IsCompleted)
    {
        Console.WriteLine($"最终结果: {notification.Result.Content}");
    }
});
```

### 4. 获取代理能力

获取代理名片以了解能力：

```csharp
var cardResolver = serviceProvider.GetRequiredService<IA2ACardResolver>();

var card = await cardResolver.ResolveCardAsync("https://remote-agent.example.com");

Console.WriteLine($"代理: {card.Name}");
Console.WriteLine($"描述: {card.Description}");
Console.WriteLine("能力:");
foreach (var capability in card.Capabilities)
{
    Console.WriteLine($"  - {capability}");
}
```

## 与路由系统集成

该插件通过 `A2AAgentHook` 自动与 BotSharp 的路由系统集成：

1. **代理发现**：当加载路由指令时，钩子会查询代理注册中心，并将发现的 A2A 代理添加到可路由代理列表中
2. **透明调用**：当路由器选择远程 A2A 代理时，钩子会自动使用 A2AClient 调用它
3. **回退机制**：如果 A2A 调用失败，系统会回退到标准路由

## 功能函数

该插件提供两个可由代理调用的函数：

### `a2a-query_agent_registry`

查询代理注册中心以查找有能力的代理：

**参数:**
- `Query` (string): 自然语言查询
- `RequiredCapabilities` (array): 所需能力列表
- `MaxResults` (int): 最大结果数

**示例:**
```json
{
  "Query": "谁可以处理发票？",
  "RequiredCapabilities": ["invoice-processing"],
  "MaxResults": 3
}
```

### `a2a-invoke_remote_agent`

调用远程 A2A 代理：

**参数:**
- `AgentId` (string): 目标代理 ID
- `Parameters` (object): 代理特定参数
- `Context` (array): 对话上下文（可选）

**示例:**
```json
{
  "AgentId": "invoice-processing-agent",
  "Parameters": {
    "invoice_file": "invoice-2024-001.pdf",
    "action": "extract_data"
  }
}
```

## 远程代理协议

### 代理名片端点

远程代理应在 `/a2a/card` 公开代理名片：

```json
{
  "Id": "tax-calculation-agent",
  "Name": "税务计算器",
  "Description": "根据收入和扣除计算税务",
  "Capabilities": ["tax-calculation", "tax-planning"],
  "Endpoint": "https://tax-agent.example.com",
  "SupportedProtocols": ["A2A/1.0"],
  "RequiredParameters": [
    {
      "Name": "income",
      "Description": "年收入",
      "Type": "number",
      "Required": true
    }
  ],
  "OptionalParameters": [
    {
      "Name": "deductions",
      "Description": "税务扣除",
      "Type": "number",
      "Required": false
    }
  ]
}
```

### 调用端点

远程代理应在 `/a2a/invoke` 接受调用：

**请求:**
```json
{
  "AgentId": "tax-calculation-agent",
  "Parameters": {
    "income": 50000,
    "deductions": 10000
  },
  "Context": [
    {
      "Role": "user",
      "Content": "计算我的税务"
    }
  ],
  "CallbackUrl": "https://botsharp.example.com/api/a2a/callback"
}
```

**响应:**
```json
{
  "RequestId": "req-12345",
  "Status": "Success",
  "Response": {
    "Role": "assistant",
    "Content": "您的预估税额为 8,000 元"
  },
  "IsAsync": false
}
```

### 进度通知

对于长时间运行的操作，远程代理可以发送进度更新：

**POST 到 CallbackUrl:**
```json
{
  "RequestId": "req-12345",
  "Progress": 50,
  "Message": "正在处理数据...",
  "IsCompleted": false
}
```

**最终通知:**
```json
{
  "RequestId": "req-12345",
  "Progress": 100,
  "Message": "分析完成",
  "IsCompleted": true,
  "Result": {
    "Role": "assistant",
    "Content": "分析结果: ..."
  }
}
```

## 优势

1. **动态路由**：路由器可以发现并连接到新代理，无需修改代码
2. **可扩展性**：在多个服务之间分布代理能力
3. **异步操作**：处理长时间运行的任务而不阻塞
4. **解耦**：代理可以独立开发和部署
5. **语义发现**：根据能力查找代理，而不仅仅是名称

## 示例场景

**用户请求**："计算我的企业收入税务"

**流程:**
1. 路由代理接收请求
2. A2AAgentHook 查询代理注册中心："谁擅长处理税务计算？"
3. 注册中心返回匹配的代理及置信度评分
4. 路由器根据置信度和上下文选择最佳代理
5. A2AClient 调用远程税务计算代理
6. 如果计算需要时间：
   - 代理返回带有 RequestId 的异步响应
   - 定期发送进度通知
   - 通过回调推送最终结果
7. 路由器接收结果并响应用户

## 许可证

此插件是 BotSharp 的一部分，遵循相同的许可条款。
