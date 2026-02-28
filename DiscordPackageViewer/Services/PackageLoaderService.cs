using System.Globalization;
using System.IO.Compression;
using System.Text;
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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

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

            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

            // Detect root prefix from the first few entries
            var rootPrefix = DetectRootPrefix(archive);

            var totalEntries = archive.Entries.Count;
            var processed = 0;

            // Scratch collections that accumulate per-channel / per-server data
            var channelMessages = new Dictionary<string, List<DiscordMessage>>(StringComparer.OrdinalIgnoreCase);
            var channelMeta = new Dictionary<string, ChannelMeta>(StringComparer.OrdinalIgnoreCase);
            var serverParts = new Dictionary<string, ServerAccumulator>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in archive.Entries)
            {
                if (entry.Length == 0 && entry.FullName.EndsWith('/'))
                {
                    processed++;
                    continue;
                }

                var path = NormalizePath(entry.FullName);
                var relative = rootPrefix.Length > 0 && path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
                    ? path[rootPrefix.Length..]
                    : path;

                // Report progress based on entry count
                processed++;
                if (processed % 50 == 0 || processed == totalEntries)
                {
                    var pct = 0.05 + 0.85 * ((double)processed / totalEntries);
                    SetProgress(pct, $"Processing {processed}/{totalEntries}…");
                }

                try
                {
                    // ── Account ────────────────────────────
                    if (relative.Equals("Account/user.json", StringComparison.OrdinalIgnoreCase))
                    {
                        data.UserProfile = Deserialize<UserProfile>(entry);
                        continue;
                    }

                    if (IsAvatarPath(relative, out var avatarMime))
                    {
                        data.AvatarBytes = ReadBytes(entry);
                        data.AvatarMimeType = avatarMime;
                        continue;
                    }

                    // ── Messages ───────────────────────────
                    if (relative.Equals("Messages/index.json", StringComparison.OrdinalIgnoreCase))
                    {
                        data.ChannelIndex = Deserialize<Dictionary<string, string>>(entry) ?? [];
                        continue;
                    }

                    if (relative.StartsWith("Messages/c", StringComparison.OrdinalIgnoreCase))
                    {
                        var channelFolder = ExtractFolder(relative, "Messages/");
                        if (channelFolder is null) continue;

                        var fileName = relative[(("Messages/" + channelFolder + "/").Length)..];

                        if (fileName.Equals("channel.json", StringComparison.OrdinalIgnoreCase))
                        {
                            channelMeta[channelFolder] = Deserialize<ChannelMeta>(entry) ?? new();
                        }
                        else if (fileName.Equals("messages.json", StringComparison.OrdinalIgnoreCase))
                        {
                            var msgs = Deserialize<List<DiscordMessage>>(entry) ?? [];
                            foreach (var msg in msgs)
                            {
                                if (DateTime.TryParseExact(msg.Timestamp, "yyyy-MM-dd HH:mm:ss",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                                    msg.ParsedTimestamp = dt;
                            }
                            msgs.Sort((a, b) => string.Compare(a.Timestamp, b.Timestamp, StringComparison.Ordinal));
                            channelMessages[channelFolder] = msgs;
                        }
                        continue;
                    }

                    // ── Servers ────────────────────────────
                    if (relative.StartsWith("Servers/", StringComparison.OrdinalIgnoreCase)
                        && !relative.Equals("Servers/index.json", StringComparison.OrdinalIgnoreCase))
                    {
                        if (relative.Equals("Servers/index.json", StringComparison.OrdinalIgnoreCase))
                        {
                            data.ServerIndex = Deserialize<Dictionary<string, string>>(entry) ?? [];
                            continue;
                        }

                        var serverFolder = ExtractFolder(relative, "Servers/");
                        if (serverFolder is null) continue;

                        if (!serverParts.TryGetValue(serverFolder, out var acc))
                        {
                            acc = new ServerAccumulator();
                            serverParts[serverFolder] = acc;
                        }

                        var sFile = relative[(("Servers/" + serverFolder + "/").Length)..];

                        if (sFile.Equals("guild.json", StringComparison.OrdinalIgnoreCase))
                            acc.Guild = Deserialize<GuildInfo>(entry);
                        else if (sFile.Equals("channels.json", StringComparison.OrdinalIgnoreCase))
                            acc.Channels = Deserialize<List<GuildChannel>>(entry) ?? [];
                        else if (sFile.Equals("audit-log.json", StringComparison.OrdinalIgnoreCase))
                            acc.AuditLog = Deserialize<List<AuditLogEntry>>(entry) ?? [];
                        else if (sFile.Equals("emoji.json", StringComparison.OrdinalIgnoreCase))
                            acc.Emojis = Deserialize<List<EmojiMeta>>(entry) ?? [];
                        else if (sFile.Equals("webhooks.json", StringComparison.OrdinalIgnoreCase))
                            acc.Webhooks = Deserialize<List<WebhookInfo>>(entry) ?? [];
                        else if (IsIconPath(sFile, out var iconMime))
                        {
                            var iconBytes = ReadBytes(entry);
                            acc.IconDataUrl = $"data:{iconMime};base64,{Convert.ToBase64String(iconBytes)}";
                        }
                        else if (sFile.StartsWith("emoji/", StringComparison.OrdinalIgnoreCase) && sFile.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            var emojiId = Path.GetFileNameWithoutExtension(sFile);
                            var emojiBytes = ReadBytes(entry);
                            acc.EmojiDataUrls[emojiId] = $"data:image/png;base64,{Convert.ToBase64String(emojiBytes)}";
                        }
                        continue;
                    }

                    if (relative.Equals("Servers/index.json", StringComparison.OrdinalIgnoreCase))
                    {
                        data.ServerIndex = Deserialize<Dictionary<string, string>>(entry) ?? [];
                        continue;
                    }

                    // ── Ads ────────────────────────────────
                    if (relative.Equals("Ads/traits.json", StringComparison.OrdinalIgnoreCase))
                    {
                        data.AdTraits = Deserialize<AdTraits>(entry);
                        continue;
                    }

                    // ── Support Tickets ────────────────────
                    if (relative.Equals("Support_Tickets/tickets.json", StringComparison.OrdinalIgnoreCase))
                    {
                        data.SupportTickets = Deserialize<Dictionary<string, SupportTicket>>(entry) ?? [];
                        continue;
                    }

                    // ── Data Exports ───────────────────────
                    if (relative.StartsWith("Account/user_data_exports/", StringComparison.OrdinalIgnoreCase)
                        && relative.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        var envelope = Deserialize<DataExportEnvelope>(entry);
                        if (envelope is not null)
                            data.DataExports.Add(envelope);
                        continue;
                    }

                    // ── Activity (skip giant files to save memory — keep under 5 MB each) ──
                    if (relative.StartsWith("Activity/", StringComparison.OrdinalIgnoreCase)
                        && relative.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                        && entry.Length < 5L * 1024 * 1024)
                    {
                        try
                        {
                            var elements = Deserialize<List<JsonElement>>(entry);
                            if (elements is not null)
                            {
                                var parts = relative.Split('/');
                                var sectionName = parts.Length >= 2 ? parts[^2] : Path.GetFileNameWithoutExtension(relative);
                                data.ActivityEvents ??= [];
                                data.ActivityEvents[sectionName] = elements;
                            }
                        }
                        catch { /* skip malformed */ }
                        continue;
                    }
                }
                catch
                {
                    // Skip individual entries that fail to parse
                }
            }

            // ── Assemble channels ──────────────────────
            SetProgress(0.92, "Assembling channels…");
            foreach (var folder in channelMessages.Keys.Union(channelMeta.Keys).Distinct())
            {
                var channelId = folder.StartsWith('c') ? folder[1..] : folder;
                var displayName = data.ChannelIndex.TryGetValue(channelId, out var name) ? name : $"Channel {channelId}";

                data.Channels[channelId] = new LoadedChannel
                {
                    ChannelId = channelId,
                    DisplayName = displayName,
                    Meta = channelMeta.GetValueOrDefault(folder),
                    Messages = channelMessages.GetValueOrDefault(folder) ?? []
                };
            }

            // ── Assemble servers ───────────────────────
            SetProgress(0.95, "Assembling servers…");
            foreach (var (folder, acc) in serverParts)
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

    // ─── Helpers ─────────────────────────────────────────

    private void SetProgress(double value, string message)
    {
        Progress = value;
        ProgressMessage = message;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    private static string NormalizePath(string path)
        => path.Replace('\\', '/').TrimStart('/');

    private static string DetectRootPrefix(ZipArchive archive)
    {
        string? commonPrefix = null;
        var sampled = 0;
        foreach (var entry in archive.Entries)
        {
            var path = NormalizePath(entry.FullName);
            var slashIndex = path.IndexOf('/');
            if (slashIndex < 0) continue;
            var prefix = path[..(slashIndex + 1)];

            if (commonPrefix is null)
                commonPrefix = prefix;
            else if (commonPrefix != prefix)
                return "";

            if (++sampled > 20) break; // enough to decide
        }
        return commonPrefix ?? "";
    }

    /// <summary>
    /// Extract the first folder name after a given base (e.g. "Messages/c123" → "c123").
    /// </summary>
    private static string? ExtractFolder(string relative, string basePath)
    {
        var rest = relative[basePath.Length..];
        var slash = rest.IndexOf('/');
        return slash > 0 ? rest[..slash] : null;
    }

    private static bool IsAvatarPath(string relative, out string mime)
    {
        mime = "";
        foreach (var (ext, m) in ImageExtensions)
        {
            if (relative.Equals($"Account/avatar.{ext}", StringComparison.OrdinalIgnoreCase))
            {
                mime = m;
                return true;
            }
        }
        return false;
    }

    private static bool IsIconPath(string fileName, out string mime)
    {
        mime = "";
        foreach (var (ext, m) in ImageExtensions)
        {
            if (fileName.Equals($"icon.{ext}", StringComparison.OrdinalIgnoreCase))
            {
                mime = m;
                return true;
            }
        }
        return false;
    }

    private static readonly (string ext, string mime)[] ImageExtensions =
    [
        ("gif", "image/gif"), ("png", "image/png"),
        ("jpg", "image/jpeg"), ("jpeg", "image/jpeg"),
        ("webp", "image/webp")
    ];

    /// <summary>Read a ZIP entry's raw bytes without intermediate buffering.</summary>
    private static byte[] ReadBytes(ZipArchiveEntry entry)
    {
        using var s = entry.Open();
        var buf = new byte[entry.Length];
        int offset = 0, read;
        while (offset < buf.Length && (read = s.Read(buf, offset, buf.Length - offset)) > 0)
            offset += read;
        return buf;
    }

    /// <summary>Deserialize a ZIP entry's JSON content directly from the stream.</summary>
    private static T? Deserialize<T>(ZipArchiveEntry entry) where T : class
    {
        try
        {
            using var s = entry.Open();
            return JsonSerializer.Deserialize<T>(s, JsonOptions);
        }
        catch
        {
            return null;
        }
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
