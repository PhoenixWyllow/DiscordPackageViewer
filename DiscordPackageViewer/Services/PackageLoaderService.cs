using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using DiscordPackageViewer.Models;

namespace DiscordPackageViewer.Services;

/// <summary>
/// Loads and parses a Discord data package from a ZIP file entirely in-browser.
/// Uses single-pass streaming to minimise peak memory — each ZIP entry is parsed
/// on the fly and the raw bytes are released immediately.
/// </summary>
public class PackageLoaderService
{
    public PackageData? Data { get; private set; }
    public bool IsLoaded => Data is not null;
    public bool IsLoading { get; private set; }
    public string? Error { get; private set; }
    public double Progress { get; private set; }
    public string ProgressMessage { get; private set; } = "";

    public event Action? OnStateChanged;

    /// <summary>
    /// Load a Discord data package from a seekable stream containing the ZIP.
    /// The stream is disposed by the caller.
    /// </summary>
    public async Task LoadFromZipAsync(Stream zipStream)
    {
        try
        {
            IsLoading = true;
            Error = null;
            Progress = 0;
            Data = null;
            NotifyStateChanged();

            SetProgress(0.05, "Opening archive…");

            var data = new PackageData();
            var scratch = new ParseScratch();

            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
            var rootPrefix = ZipHelpers.DetectRootPrefix(archive);

            ProcessAllEntries(archive, rootPrefix, data, scratch);

            SetProgress(0.92, "Assembling channels…");
            AssembleChannels(data, scratch);

            SetProgress(0.95, "Assembling servers…");
            AssembleServers(data, scratch);

            SetProgress(1.0, "Done!");
            Data = data;
        }
        catch (Exception ex)
        {
            Error = $"Failed to load package: {ex.Message}";
            Data = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public void Unload()
    {
        Data = null;
        Error = null;
        Progress = 0;
        ProgressMessage = "";
        NotifyStateChanged();
    }

    // ─── Entry routing ───────────────────────────────────

    private void ProcessAllEntries(ZipArchive archive, string rootPrefix, PackageData data, ParseScratch scratch)
    {
        var totalEntries = archive.Entries.Count;
        var processed = 0;

        foreach (var entry in archive.Entries)
        {
            if (entry.Length == 0 && entry.FullName.EndsWith('/'))
            {
                processed++;
                continue;
            }

            var path = ZipHelpers.NormalizePath(entry.FullName);
            var relative = rootPrefix.Length > 0 && path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
                ? path[rootPrefix.Length..]
                : path;

            processed++;
            if (processed % 50 == 0 || processed == totalEntries)
            {
                var pct = 0.05 + 0.85 * ((double)processed / totalEntries);
                SetProgress(pct, $"Processing {processed}/{totalEntries}…");
            }

            try { RouteEntry(relative, entry, data, scratch); }
            catch { /* Skip individual entries that fail to parse */ }
        }
    }

    private static void RouteEntry(string relative, ZipArchiveEntry entry, PackageData data, ParseScratch scratch)
    {
        // Extract top-level folder: "Servers/guild.json" → "servers"
        var slashIdx = relative.IndexOf('/');
        if (slashIdx < 0) return;
        var topFolder = relative[..slashIdx].ToLowerInvariant();

        switch (topFolder)
        {
            case "account":         ParseAccount(relative, entry, data); break;
            case "messages":        ParseMessages(relative, entry, data, scratch); break;
            case "servers":         ParseServers(relative, entry, data, scratch); break;
            case "ads":             ParseAds(relative, entry, data); break;
            case "support_tickets": ParseSupport(relative, entry, data); break;
            case "activity":        ParseActivity(relative, entry, data); break;
        }
    }

    // ─── Section parsers ─────────────────────────────────

    private static void ParseAccount(string relative, ZipArchiveEntry entry, PackageData data)
    {
        var fileName = relative["Account/".Length..];

        switch (fileName.ToLowerInvariant())
        {
            case "user.json":
                data.UserProfile = ZipHelpers.Deserialize<UserProfile>(entry);
                break;

            default:
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                if (nameWithoutExt.Equals("avatar", StringComparison.OrdinalIgnoreCase)
                    && ZipHelpers.TryGetImageMime(fileName, out var avatarMime))
                {
                    // Account/avatar.png, .gif, etc.
                    data.AvatarBytes = ZipHelpers.ReadBytes(entry);
                    data.AvatarMimeType = avatarMime;
                }
                else if (fileName.StartsWith("user_data_exports/", StringComparison.OrdinalIgnoreCase)
                         && fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var envelope = ZipHelpers.Deserialize<DataExportEnvelope>(entry);
                    if (envelope is not null)
                        data.DataExports.Add(envelope);
                }
                break;
        }
    }

    private static void ParseMessages(string relative, ZipArchiveEntry entry, PackageData data, ParseScratch scratch)
    {
        var afterMessages = relative["Messages/".Length..];

        // Messages/index.json — channel name lookup
        if (afterMessages.Equals("index.json", StringComparison.OrdinalIgnoreCase))
        {
            data.ChannelIndex = ZipHelpers.Deserialize<Dictionary<string, string>>(entry) ?? [];
            return;
        }

        // Messages/c<id>/<file>.json — per-channel data
        if (!afterMessages.StartsWith('c')) return;

        var channelFolder = ZipHelpers.ExtractFolder(relative, "Messages/");
        if (channelFolder is null) return;

        var fileName = afterMessages[(channelFolder.Length + 1)..];

        switch (fileName.ToLowerInvariant())
        {
            case "channel.json":
                scratch.ChannelMeta[channelFolder] = ZipHelpers.Deserialize<ChannelMeta>(entry) ?? new();
                break;

            case "messages.json":
                var msgs = ZipHelpers.Deserialize<List<DiscordMessage>>(entry) ?? [];
                foreach (var msg in msgs)
                {
                    if (DateTime.TryParseExact(msg.Timestamp, "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        msg.ParsedTimestamp = dt;
                }
                msgs.Sort((a, b) => string.Compare(a.Timestamp, b.Timestamp, StringComparison.Ordinal));
                scratch.ChannelMessages[channelFolder] = msgs;
                break;
        }
    }

    private static void ParseServers(string relative, ZipArchiveEntry entry, PackageData data, ParseScratch scratch)
    {
        var afterServers = relative["Servers/".Length..];

        // Servers/index.json — server name lookup
        if (afterServers.Equals("index.json", StringComparison.OrdinalIgnoreCase))
        {
            data.ServerIndex = ZipHelpers.Deserialize<Dictionary<string, string>>(entry) ?? [];
            return;
        }

        var serverFolder = ZipHelpers.ExtractFolder(relative, "Servers/");
        if (serverFolder is null) return;

        if (!scratch.ServerParts.TryGetValue(serverFolder, out var acc))
        {
            acc = new ServerAccumulator();
            scratch.ServerParts[serverFolder] = acc;
        }

        var sFile = afterServers[(serverFolder.Length + 1)..];

        switch (sFile.ToLowerInvariant())
        {
            case "guild.json":     acc.Guild    = ZipHelpers.Deserialize<GuildInfo>(entry); break;
            case "channels.json":  acc.Channels = ZipHelpers.Deserialize<List<GuildChannel>>(entry) ?? []; break;
            case "audit-log.json": acc.AuditLog = ZipHelpers.Deserialize<List<AuditLogEntry>>(entry) ?? []; break;
            case "emoji.json":     acc.Emojis   = ZipHelpers.Deserialize<List<EmojiMeta>>(entry) ?? []; break;
            case "webhooks.json":  acc.Webhooks = ZipHelpers.Deserialize<List<WebhookInfo>>(entry) ?? []; break;

            default:
                // icon.png / icon.gif / … — server icon
                var iconName = Path.GetFileNameWithoutExtension(sFile);
                if (iconName.Equals("icon", StringComparison.OrdinalIgnoreCase)
                    && ZipHelpers.TryGetImageMime(sFile, out var iconMime))
                {
                    acc.IconDataUrl = ZipHelpers.ReadDataUrl(entry, iconMime);
                }
                // emoji/<id>.png — custom emoji
                else if (sFile.StartsWith("emoji/", StringComparison.OrdinalIgnoreCase)
                         && sFile.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    var emojiId = Path.GetFileNameWithoutExtension(sFile);
                    acc.EmojiDataUrls[emojiId] = ZipHelpers.ReadDataUrl(entry, "image/png");
                }
                break;
        }
    }

    private static void ParseAds(string relative, ZipArchiveEntry entry, PackageData data)
    {
        if (relative["Ads/".Length..].Equals("traits.json", StringComparison.OrdinalIgnoreCase))
            data.AdTraits = ZipHelpers.Deserialize<AdTraits>(entry);
    }

    private static void ParseSupport(string relative, ZipArchiveEntry entry, PackageData data)
    {
        if (relative["Support_Tickets/".Length..].Equals("tickets.json", StringComparison.OrdinalIgnoreCase))
            data.SupportTickets = ZipHelpers.Deserialize<Dictionary<string, SupportTicket>>(entry) ?? [];
    }

    private static void ParseActivity(string relative, ZipArchiveEntry entry, PackageData data)
    {
        if (!relative.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            || entry.Length >= 5L * 1024 * 1024)
        {
            return;
        }

        try
        {
            var elements = ZipHelpers.Deserialize<List<JsonElement>>(entry);
            if (elements is not null)
            {
                var parts = relative.Split('/');
                var sectionName = parts.Length >= 2 ? parts[^2] : Path.GetFileNameWithoutExtension(relative);
                data.ActivityEvents ??= [];
                data.ActivityEvents[sectionName] = elements;
            }
        }
        catch { /* skip malformed */ }
    }

    // ─── Post-processing assembly ────────────────────────

    private static void AssembleChannels(PackageData data, ParseScratch scratch)
    {
        foreach (var folder in scratch.ChannelMessages.Keys.Union(scratch.ChannelMeta.Keys).Distinct())
        {
            var channelId = folder.StartsWith('c') ? folder[1..] : folder;
            var displayName = data.ChannelIndex.TryGetValue(channelId, out var name) ? name : $"Channel {channelId}";

            data.Channels[channelId] = new LoadedChannel
            {
                ChannelId = channelId,
                DisplayName = displayName,
                Meta = scratch.ChannelMeta.GetValueOrDefault(folder),
                Messages = scratch.ChannelMessages.GetValueOrDefault(folder) ?? []
            };
        }
    }

    private static void AssembleServers(PackageData data, ParseScratch scratch)
    {
        foreach (var (folder, acc) in scratch.ServerParts)
        {
            var displayName = data.ServerIndex.TryGetValue(folder, out var sname) ? sname : acc.Guild?.Name ?? folder;
            data.Servers[folder] = new LoadedServer
            {
                ServerId = folder,
                DisplayName = displayName,
                Guild = acc.Guild,
                Channels = acc.Channels,
                AuditLog = acc.AuditLog,
                Emojis = acc.Emojis,
                Webhooks = acc.Webhooks,
                IconDataUrl = acc.IconDataUrl,
                EmojiDataUrls = acc.EmojiDataUrls
            };
        }
    }

    // ─── Helpers ─────────────────────────────────────────

    private void SetProgress(double value, string message)
    {
        Progress = value;
        ProgressMessage = message;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    // ─── Internal types ──────────────────────────────────

    /// <summary>Scratch state accumulated during the single-pass ZIP scan.</summary>
    private sealed class ParseScratch
    {
        public Dictionary<string, List<DiscordMessage>> ChannelMessages { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ChannelMeta> ChannelMeta { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ServerAccumulator> ServerParts { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Temporary accumulator for server data during single-pass extraction.</summary>
    private sealed class ServerAccumulator
    {
        public GuildInfo? Guild;
        public List<GuildChannel> Channels = [];
        public List<AuditLogEntry> AuditLog = [];
        public List<EmojiMeta> Emojis = [];
        public List<WebhookInfo> Webhooks = [];
        public string? IconDataUrl;
        public Dictionary<string, string> EmojiDataUrls = [];
    }
}
