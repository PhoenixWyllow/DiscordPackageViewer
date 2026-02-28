using System.Text.Json.Serialization;

namespace DiscordPackageViewer.Models;

public class DiscordMessage
{
    [JsonPropertyName("ID")]
    public long Id { get; set; }

    [JsonPropertyName("Timestamp")]
    public string Timestamp { get; set; } = "";

    [JsonPropertyName("Contents")]
    public string Contents { get; set; } = "";

    [JsonPropertyName("Attachments")]
    public string Attachments { get; set; } = "";

    /// <summary>Parsed timestamp for sorting/charting. Populated after deserialization.</summary>
    [JsonIgnore]
    public DateTime? ParsedTimestamp { get; set; }
}

public class ChannelMeta
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>Only present for guild channels.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Only present for guild channels.</summary>
    [JsonPropertyName("guild")]
    public ChannelGuildInfo? Guild { get; set; }

    /// <summary>Only present for DM channels.</summary>
    [JsonPropertyName("recipients")]
    public List<string>? Recipients { get; set; }
}

public class ChannelGuildInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

/// <summary>
/// Represents a loaded channel with its metadata, messages, and display name.
/// </summary>
public class LoadedChannel
{
    public string ChannelId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public ChannelMeta? Meta { get; set; }
    public List<DiscordMessage> Messages { get; set; } = [];
    public int MessageCount => Messages.Count;

    /// <summary>True if this is a DM channel.</summary>
    public bool IsDm => Meta?.Type == "DM";

    /// <summary>Server name if this is a guild channel.</summary>
    public string? ServerName => Meta?.Guild?.Name;
}
