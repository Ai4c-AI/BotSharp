using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BotSharp.Plugin.AgentSkills;

public class SkillLoaderService : IDisposable
{
    private readonly AgentSkillsSettings _settings;
    private readonly ILogger<SkillLoaderService> _logger;
    private readonly Dictionary<string, AgentSkillDef> _skills = new();
    private FileSystemWatcher _watcher;
    private readonly IDeserializer _deserializer;

    public SkillLoaderService(AgentSkillsSettings settings, ILogger<SkillLoaderService> logger)
    {
        _settings = settings;
        _logger = logger;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        Initialize();
    }

    private void Initialize()
    {
        if (string.IsNullOrEmpty(_settings.SkillsDir) || !Directory.Exists(_settings.SkillsDir))
        {
            _logger.LogWarning($"Skills directory '{_settings.SkillsDir}' not found.");
            return;
        }

        LoadSkills();
        InitWatcher();
    }

    private void LoadSkills()
    {
        lock (_skills)
        {
            _skills.Clear();
        }
        
        var files = Directory.GetFiles(_settings.SkillsDir, "SKILL.md", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            LoadSkill(file);
        }
    }

    private void LoadSkill(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            if (!content.StartsWith("---"))
            {
                _logger.LogWarning($"Invalid skill file format (missing frontmatter): {filePath}");
                return;
            }

            var parts = content.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                 _logger.LogWarning($"Invalid skill file format (missing body): {filePath}");
                 return;
            }

            var yaml = parts[0];
            var body = string.Join("---", parts.Skip(1)).Trim();

            var skill = _deserializer.Deserialize<AgentSkillDef>(yaml);
            if (skill != null)
            {
                skill.Instructions = body;
                skill.BasePath = Path.GetDirectoryName(filePath);

                var scriptsPath = Path.Combine(skill.BasePath, "scripts");
                if (Directory.Exists(scriptsPath))
                {
                    skill.ScriptFiles = Directory.GetFiles(scriptsPath).ToList();
                }
                else
                {
                    skill.ScriptFiles = new List<string>();
                }

                var resourcesPath = Path.Combine(skill.BasePath, "resources");
                if (Directory.Exists(resourcesPath))
                {
                    skill.ResourceFiles = Directory.GetFiles(resourcesPath).ToList();
                }
                else
                {
                    skill.ResourceFiles = new List<string>();
                }

                lock (_skills)
                {
                    _skills[skill.Name] = skill;
                }
                
                _logger.LogInformation($"Loaded skill: {skill.Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading skill from {filePath}");
        }
    }

    private void InitWatcher()
    {
        _watcher = new FileSystemWatcher(_settings.SkillsDir);
        _watcher.IncludeSubdirectories = true;
        _watcher.Filter = "SKILL.md";
        _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;

        _watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"Skill file changed: {e.FullPath}");
        // Simple debounce
        Thread.Sleep(100); 
        LoadSkill(e.FullPath);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
         _logger.LogInformation($"Skill file deleted: {e.FullPath}");
         // Re-scan all to ensure consistency
         LoadSkills();
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation($"Skill file renamed: {e.OldFullPath} to {e.FullPath}");
        LoadSkills();
    }

    public AgentSkillDef? GetSkill(string name)
    {
        lock (_skills)
        {
            return _skills.TryGetValue(name, out var skill) ? skill : null;
        }
    }
    
    public IEnumerable<AgentSkillDef> GetSkills()
    {
        lock (_skills)
        {
            return _skills.Values.ToList();
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }

    public IEnumerable<SkillMetadata> GetAllSkillsMetadata()
    {
        lock (_skills)
        {
            return _skills.Values.Select(x => new SkillMetadata
            {
                Name = x.Name,
                Description = x.Description
            }).ToList();
        }
    }
}

public class SkillMetadata
{
    public string Name { get; set; }
    public string Description { get; set; }
}