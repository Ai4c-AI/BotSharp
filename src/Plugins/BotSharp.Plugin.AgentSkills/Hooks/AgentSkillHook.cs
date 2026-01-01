using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Agents.Enums;
using BotSharp.Abstraction.Agents.Models;
using BotSharp.Abstraction.Agents.Settings;
using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Functions.Models;
using BotSharp.Abstraction.Messaging;
using BotSharp.Abstraction.Messaging.Models.RichContent;
using BotSharp.Abstraction.Messaging.Models.RichContent.Template;
using BotSharp.Abstraction.Routing.Models;
using BotSharp.Plugin.AgentSkills;
using BotSharp.Plugin.PythonInterpreter.Services;
using Microsoft.Extensions.DependencyInjection;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotSharp.Plugin.AgentSkills.Hooks;

public class AgentSkillHook : AgentHookBase
{
    public override string SelfId => "";

    private readonly SkillLoaderService _loader;
    private readonly IConversationStateService _state;
    private readonly PythonScriptExecutor _pythonExecutor;

    public AgentSkillHook(IServiceProvider services, AgentSettings settings) : base(services, settings)
    {
        _loader = services.GetRequiredService<SkillLoaderService>();
        _state = services.GetRequiredService<IConversationStateService>();
        _pythonExecutor = services.GetRequiredService<PythonScriptExecutor>();
    }

    public override void OnAgentLoaded(Agent agent)
    {
       
    }

    public override bool OnInstructionLoaded(string template, IDictionary<string, object> dict)
    {
        // 1. 获取当前激活的技能列表
        var activeSkills = _state.GetState("active_skills")?.Split(',') ?? new string[0];

        // 2. 注入已激活技能的详细指令 (SKILL.md 正文)
        if (activeSkills.Any())
        {
            var skillInstructions = new StringBuilder();
            foreach (var skillName in activeSkills)
            {
                var skill = _loader.GetSkill(skillName);
                skillInstructions.AppendLine($"\n### SKILL: {skill.Name}");
                skillInstructions.AppendLine(skill.Instructions);
            }
            // 将技能指令拼接到系统 Prompt 中
            template += skillInstructions.ToString();
        }

        // 3. 注入技能发现菜单 (仅包含 Name 和 Description)
        // 始终保留此菜单，以便 Agent 可以随时切换或激活新技能
        var allSkills = _loader.GetAllSkillsMetadata();
        var menu = string.Join("\n", allSkills.Select(s => $"- {s.Name}: {s.Description}"));
        template += $"\n\n\n{menu}\nUse 'enable_skill' to activate one.";

        return base.OnInstructionLoaded(template, dict);
    }

    public override bool OnFunctionsLoaded(List<FunctionDef> functions)
    {
        var properties = new Dictionary<string, object>
            {
                {
                    "skill_name",
                    new
                    {
                        type = "string",
                        description = "The name of the skill to enable."
                    }
                }
            };

        var propertiesJson = JsonSerializer.Serialize(properties);
        var propertiesDocument = JsonDocument.Parse(propertiesJson);

        // 注册核心管理工具
        functions.Add(new FunctionDef
        {
            Name = "enable_skill",
            Description = "Activates a skill by name to access its specialized instructions and tools.",
            Parameters = new FunctionParametersDef
            {
                Type = "object",
                Properties = propertiesDocument,
                Required = new List<string> { "skill_name" }
            }
        });

        // 动态注册脚本为工具
        var activeSkills = _state.GetState("active_skills")?.Split(',') ?? new string[0];
        foreach (var skillName in activeSkills)
        {
            var skill = _loader.GetSkill(skillName);
            foreach (var script in skill.ScriptFiles)
            {
                functions.Add(new FunctionDef
                {
                    Name = Path.GetFileNameWithoutExtension(script),
                    Description = $"Executes script {script} for skill {skillName}",
                    // 参数定义需要从外部元数据或代码解析中获取
                    Parameters = GetParametersForScript(script)
                });
            }
        }        

        return base.OnFunctionsLoaded(functions);
    }

    private FunctionParametersDef GetParametersForScript(string script)
    {
        if (!File.Exists(script))
        {
            return new FunctionParametersDef();
        }

        try
        {
            if (!PythonEngine.IsInitialized)
            {
                return new FunctionParametersDef();
            }

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();
                var scriptContent = File.ReadAllText(script);
                var functionName = Path.GetFileNameWithoutExtension(script);

                scope.Exec(@"
import ast
import json

def get_args(code, func_name):
    try:
        tree = ast.parse(code)
        for node in tree.body:
            if isinstance(node, ast.FunctionDef) and node.name == func_name:
                params = {}
                required = []
                for arg in node.args.args:
                    arg_name = arg.arg
                    arg_type = 'string'
                    
                    if arg.annotation:
                        if isinstance(arg.annotation, ast.Name):
                            if arg.annotation.id == 'int': arg_type = 'integer'
                            elif arg.annotation.id == 'float': arg_type = 'number'
                            elif arg.annotation.id == 'bool': arg_type = 'boolean'
                            elif arg.annotation.id == 'str': arg_type = 'string'
                            elif arg.annotation.id == 'dict': arg_type = 'object'
                            elif arg.annotation.id == 'list': arg_type = 'array'
                    
                    params[arg_name] = {
                        'type': arg_type,
                        'description': ''
                    }
                    required.append(arg_name)
                
                return json.dumps({'properties': params, 'required': required})
    except Exception as e:
        return json.dumps({})
    return json.dumps({})
");
                dynamic get_args = scope.Get("get_args");
                string jsonResult = get_args(scriptContent, functionName);

                if (!string.IsNullOrEmpty(jsonResult) && jsonResult != "{}")
                {
                    var doc = JsonDocument.Parse(jsonResult);
                    if (doc.RootElement.TryGetProperty("properties", out var props))
                    {
                        var required = new List<string>();
                        if (doc.RootElement.TryGetProperty("required", out var req))
                        {
                            foreach (var item in req.EnumerateArray())
                            {
                                required.Add(item.GetString());
                            }
                        }

                        return new FunctionParametersDef
                        {
                            Type = "object",
                            Properties = JsonDocument.Parse(props.GetRawText()),
                            Required = required
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing python script {script}: {ex.Message}");
        }

        return new FunctionParametersDef
        {
            Type = "object",
            Properties = JsonDocument.Parse("{}")
        };
    }
}
