using System.Text.Json.Serialization;

namespace BotSharp.Plugin.MicrosoftAgentFramework.Models;

/// <summary>
/// Request model for order processing workflow
/// </summary>
public class OrderRequest
{
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; } = new();

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "process"; // process, refund, cancel

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

/// <summary>
/// Order item details
/// </summary>
public class OrderItem
{
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
