using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Plugin.AgentSkills;

public class AgentSkillDef
{
    // 映射 YAML Frontmatter
    public string Name { get; set; }
    public string Description { get; set; }

    // 映射 Markdown Body
    public string Instructions { get; set; }

    // 映射文件系统路径
    public string BasePath { get; set; }
    public List<string> ScriptFiles { get; set; }
    public List<string> ResourceFiles { get; set; }
}

