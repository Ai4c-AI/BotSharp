using BotSharp.Core;
using BotSharp.OpenAPI;
using BotSharp.Logger;
using BotSharp.Plugin.ChatHub;
using Serilog;
using BotSharp.AspNetCore.Extensions;
using BotSharp.Abstraction.Messaging.JsonConverters;
using Python.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

string[] allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[]
    {
        "http://0.0.0.0:5015",
        "https://botsharp.scisharpstack.org",
        "https://chat.scisharpstack.org"
    };
builder.Services.AddControllers();
// Add service defaults & Aspire components.
builder.AddServiceDefaults();
// Add BotSharp
builder.Services.AddBotSharpAspNetCore(builder.Configuration)
    .AddBotSharpOpenAPI(builder.Configuration, allowedOrigins, builder.Environment, true)
   .AddBotSharpLogger(builder.Configuration);



// Add SignalR for WebSocket
builder.Services.AddSignalR();

var app = builder.Build();

// Enable SignalR
app.MapHub<SignalRHub>("/chatHub");
app.UseMiddleware<WebSocketsMiddleware>();

// Use BotSharp
app.UseBotSharpCore()
    .UseBotSharpOpenAPI(app.Environment)
    .UseBotSharpUI();

//Runtime.PythonDLL = @"C:\Users\xxx\AppData\Local\Programs\Python\Python311\python311.dll";
//PythonEngine.Initialize();
//PythonEngine.BeginAllowThreads();

app.Run();

// Shut down the Python engine
//PythonEngine.Shutdown();