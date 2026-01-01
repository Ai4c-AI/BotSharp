using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BotSharp.Core.Drasi.Models;

public class DrasiChangeMessage
{
    [JsonPropertyName("op")]
    public string Operation { get; set; } // 枚举值: "i" (Insert), "u" (Update), "d" (Delete)

    [JsonPropertyName("ts_ms")]
    public long TimestampMs { get; set; }

    [JsonPropertyName("payload")]
    public DrasiPayload Payload { get; set; }
}

public class DrasiPayload
{
    [JsonPropertyName("source")]
    public DrasiSourceMetadata Source { get; set; }

    // 变更后的数据状态 (Insert/Update 时非空)
    // 使用 Dictionary<string, object> 以适应动态 Schema
    [JsonPropertyName("after")]
    public Dictionary<string, object> After { get; set; }

    // 变更前的数据状态 (Update/Delete 时非空)
    [JsonPropertyName("before")]
    public Dictionary<string, object> Before { get; set; }
}

public class DrasiSourceMetadata
{
    // 关键字段：QueryId 标识了这是哪个持续查询的结果
    [JsonPropertyName("queryId")]
    public string QueryId { get; set; }

    [JsonPropertyName("ts_ms")]
    public long TimestampMs { get; set; }
}