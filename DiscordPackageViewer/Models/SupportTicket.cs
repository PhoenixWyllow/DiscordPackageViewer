using System.Text.Json.Serialization;

namespace DiscordPackageViewer.Models;

public class SupportTicket
{
    [JsonPropertyName("ticket_id")]
    public long TicketId { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = "";

    [JsonPropertyName("comments")]
    public List<TicketComment> Comments { get; set; } = [];
}

public class TicketComment
{
    [JsonPropertyName("author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = "";

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = "";
}
