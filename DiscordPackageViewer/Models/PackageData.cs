namespace DiscordPackageViewer.Models;

/// <summary>
/// Root container for all parsed data from a Discord data package.
/// </summary>
public class PackageData
{
    // Account
    public UserProfile? UserProfile { get; set; }
    public byte[]? AvatarBytes { get; set; }
    public string? AvatarMimeType { get; set; }

    // Messages
    public Dictionary<string, string> ChannelIndex { get; set; } = [];
    public Dictionary<string, LoadedChannel> Channels { get; set; } = [];

    // Servers
    public Dictionary<string, string> ServerIndex { get; set; } = [];
    public Dictionary<string, LoadedServer> Servers { get; set; } = [];

    // Ads
    public AdTraits? AdTraits { get; set; }

    // Support
    public Dictionary<string, SupportTicket> SupportTickets { get; set; } = [];

    // Data Exports (billing, promotions, etc.)
    public List<DataExportEnvelope> DataExports { get; set; } = [];

    // Activity (large files — loaded on demand)
    public Dictionary<string, List<System.Text.Json.JsonElement>>? ActivityEvents { get; set; }

    // Summary stats (computed after loading)
    public int TotalMessages => Channels.Values.Sum(c => c.MessageCount);
    public int TotalChannels => Channels.Count;
    public int TotalServers => ServerIndex.Count;
    public int TotalFriends => UserProfile?.Relationships?.Count(r => string.Equals(r.Type, "FRIEND", StringComparison.OrdinalIgnoreCase)) ?? 0;
}
