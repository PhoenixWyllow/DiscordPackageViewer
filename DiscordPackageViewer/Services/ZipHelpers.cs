using System.IO.Compression;
using System.Text.Json;

namespace DiscordPackageViewer.Services;

/// <summary>
/// Pure static helpers for reading and deserializing ZIP archive entries.
/// Extracted from <see cref="PackageLoaderService"/> to keep it focused on orchestration.
/// </summary>
internal static class ZipHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly Dictionary<string, string> ImageMimeByExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gif"]  = "image/gif",
        ["png"]  = "image/png",
        ["jpg"]  = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        ["webp"] = "image/webp",
    };

    // ─── Path helpers ────────────────────────────────────

    public static string NormalizePath(string path)
        => path.Replace('\\', '/').TrimStart('/');

    /// <summary>
    /// Detect a common root folder prefix shared by all entries (e.g. "Discord-package/").
    /// Returns an empty string when entries sit directly at the ZIP root.
    /// </summary>
    public static string DetectRootPrefix(ZipArchive archive)
    {
        string? commonPrefix = null;
        var sampled = 0;
        foreach (var entry in archive.Entries)
        {
            var path = NormalizePath(entry.FullName);
            var slashIndex = path.IndexOf('/');
            if (slashIndex < 0)
            {
                continue;
            }

            var prefix = path[..(slashIndex + 1)];

            if (commonPrefix is null)
            {
                commonPrefix = prefix;
            }
            else if (commonPrefix != prefix)
            {
                return "";
            }

            if (++sampled > 20)
            {
                break;
            }
        }
        return commonPrefix ?? "";
    }

    /// <summary>
    /// Extract the first folder name after a given base (e.g. "Messages/c123/..." → "c123").
    /// </summary>
    public static string? ExtractFolder(string relative, string basePath)
    {
        var rest = relative[basePath.Length..];
        var slash = rest.IndexOf('/');
        return slash > 0 ? rest[..slash] : null;
    }

    // ─── Image detection ─────────────────────────────────

    /// <summary>
    /// Check whether <paramref name="fileName"/> is a known image file (e.g. "icon.png", "avatar.gif").
    /// Returns the corresponding MIME type via <paramref name="mime"/> if matched.
    /// </summary>
    public static bool TryGetImageMime(string fileName, out string mime)
    {
        mime = "";
        var ext = Path.GetExtension(fileName);
        if (ext.Length > 1 && ImageMimeByExt.TryGetValue(ext[1..], out var m))
        {
            mime = m;
            return true;
        }
        return false;
    }

    // ─── I/O ─────────────────────────────────────────────

    /// <summary>Read a ZIP entry's raw bytes without intermediate buffering.</summary>
    public static byte[] ReadBytes(ZipArchiveEntry entry)
    {
        using var s = entry.Open();
        var buf = new byte[entry.Length];
        int offset = 0, read;
        while (offset < buf.Length && (read = s.Read(buf, offset, buf.Length - offset)) > 0)
        {
            offset += read;
        }

        return buf;
    }

    /// <summary>Read a ZIP entry and return it as a base64 data-URL.</summary>
    public static string ReadDataUrl(ZipArchiveEntry entry, string mime)
        => $"data:{mime};base64,{Convert.ToBase64String(ReadBytes(entry))}";

    /// <summary>Deserialize a ZIP entry's JSON content directly from the stream.</summary>
    public static T? Deserialize<T>(ZipArchiveEntry entry) where T : class
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
}
