namespace DiscordPackageViewer;

/// <summary>
/// Central place for app-wide constants (GitHub repo URL, version, etc.).
/// </summary>
public static class AppConstants
{
    /// <summary>GitHub repository URL. Update this once your repo is created.</summary>
    public const string GitHubRepo = "https://github.com/user/DiscordPackageViewer";

    /// <summary>URL to open a new pre-filled bug report issue.</summary>
    public static string NewIssueUrl(string title, string body)
    {
        var encodedTitle = Uri.EscapeDataString(title);
        var encodedBody = Uri.EscapeDataString(body);
        return $"{GitHubRepo}/issues/new?labels=bug&title={encodedTitle}&body={encodedBody}";
    }
}
