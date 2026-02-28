using System.Text.Json.Serialization;

namespace DiscordPackageViewer.Models;

public class GuildInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("verification_level")]
    public int? VerificationLevel { get; set; }

    [JsonPropertyName("features")]
    public List<string>? Features { get; set; }

    [JsonPropertyName("roles")]
    public Dictionary<string, GuildRole>? Roles { get; set; }
}

public class GuildRole
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("color")]
    public int Color { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("permissions")]
    public long Permissions { get; set; }

    [JsonPropertyName("mentionable")]
    public bool Mentionable { get; set; }

    [JsonPropertyName("hoist")]
    public bool Hoist { get; set; }
}

public class GuildChannel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("position")]
    public int? Position { get; set; }

    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }
}

public class AuditLogEntry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("action_type")]
    public int? ActionType { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("target_id")]
    public string? TargetId { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class EmojiMeta
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("animated")]
    public bool? Animated { get; set; }
}

public class WebhookInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
}

/// <summary>
/// Represents a fully loaded server with all its data.
/// </summary>
public class LoadedServer
{
    public string ServerId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public GuildInfo? Guild { get; set; }
    public List<GuildChannel> Channels { get; set; } = [];
    public List<AuditLogEntry> AuditLog { get; set; } = [];
    public List<EmojiMeta> Emojis { get; set; } = [];
    public List<WebhookInfo> Webhooks { get; set; } = [];
    public string? IconDataUrl { get; set; }
    public Dictionary<string, string> EmojiDataUrls { get; set; } = [];
}
