using System.Reflection;

[assembly: AssemblyMetadata("ProjectUrl", DiscordPackageViewer.AppConstants.GitHubRepo)]
[assembly: AssemblyMetadata("RepositoryUrl", DiscordPackageViewer.AppConstants.GitHubRepo)]
[assembly: AssemblyMetadata("RepositoryType", "git")]

namespace DiscordPackageViewer;

/// <summary>
/// Central place for app-wide constants (GitHub repo URL, version, etc.).
/// </summary>
public static class AppConstants
{
    /// <summary>GitHub repository URL.</summary>
    public const string GitHubRepo = "https://github.com/PhoenixWyllow/DiscordPackageViewer";

    /// <summary>URL to open a new pre-filled bug report issue.</summary>
    public static string NewIssueUrl(string title, string body)
    {
        var encodedTitle = Uri.EscapeDataString(title);
        var encodedBody = Uri.EscapeDataString(body);
        return $"{GitHubRepo}/issues/new?labels=bug&title={encodedTitle}&body={encodedBody}";
    }
}
