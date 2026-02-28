using System.Text.Json.Serialization;

namespace DiscordPackageViewer.Models;

public class UserProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("discriminator")]
    public int Discriminator { get; set; }

    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("avatar_hash")]
    public string? AvatarHash { get; set; }

    [JsonPropertyName("has_mobile")]
    public bool HasMobile { get; set; }

    [JsonPropertyName("needs_email_verification")]
    public bool NeedsEmailVerification { get; set; }

    [JsonPropertyName("premium_until")]
    public string? PremiumUntil { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("temp_banned_until")]
    public string? TempBannedUntil { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("flags")]
    public List<string> Flags { get; set; } = [];

    [JsonPropertyName("connections")]
    public List<Connection> Connections { get; set; } = [];

    [JsonPropertyName("user_sessions")]
    public List<UserSession> UserSessions { get; set; } = [];

    [JsonPropertyName("relationships")]
    public List<Relationship> Relationships { get; set; } = [];

    [JsonPropertyName("guild_settings")]
    public List<GuildSetting> GuildSettings { get; set; } = [];

    [JsonPropertyName("user_activity_application_statistics")]
    public List<ActivityStat> UserActivityApplicationStatistics { get; set; } = [];

    [JsonPropertyName("user_profile_metadata")]
    public UserProfileMetadata? UserProfileMetadata { get; set; }

    [JsonPropertyName("current_orbs_balance")]
    public int CurrentOrbsBalance { get; set; }

    [JsonPropertyName("notes")]
    public Dictionary<string, string>? Notes { get; set; }
}

public class Connection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("friend_sync")]
    public bool FriendSync { get; set; }

    [JsonPropertyName("show_activity")]
    public bool ShowActivity { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("two_way_link")]
    public bool TwoWayLink { get; set; }

    [JsonPropertyName("metadata_visibility")]
    public int MetadataVisibility { get; set; }

    [JsonPropertyName("revoked")]
    public bool Revoked { get; set; }
}

public class UserSession
{
    [JsonPropertyName("id_hash")]
    public string IdHash { get; set; } = "";

    [JsonPropertyName("user_data")]
    public SessionUserData? UserData { get; set; }

    [JsonPropertyName("is_soft_deleted")]
    public bool IsSoftDeleted { get; set; }
}

public class SessionUserData
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("creation_time")]
    public string? CreationTime { get; set; }

    [JsonPropertyName("expiration_time")]
    public string? ExpirationTime { get; set; }

    [JsonPropertyName("approx_last_used_time")]
    public string? ApproxLastUsedTime { get; set; }

    [JsonPropertyName("is_mfa")]
    public bool IsMfa { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("client_info")]
    public ClientInfo? ClientInfo { get; set; }
}

public class ClientInfo
{
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("os")]
    public string? Os { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }
}

public class Relationship
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("user_ignored")]
    public bool UserIgnored { get; set; }

    [JsonPropertyName("user")]
    public RelationshipUser? User { get; set; }

    [JsonPropertyName("is_spam_request")]
    public bool IsSpamRequest { get; set; }
}

public class RelationshipUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}

public class GuildSetting
{
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("suppress_everyone")]
    public bool SuppressEveryone { get; set; }

    [JsonPropertyName("suppress_roles")]
    public bool SuppressRoles { get; set; }

    [JsonPropertyName("mute_scheduled_events")]
    public bool MuteScheduledEvents { get; set; }

    [JsonPropertyName("message_notifications")]
    public int MessageNotifications { get; set; }

    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("mobile_push")]
    public bool MobilePush { get; set; }

    [JsonPropertyName("muted")]
    public bool Muted { get; set; }

    [JsonPropertyName("hide_muted_channels")]
    public bool HideMutedChannels { get; set; }

    [JsonPropertyName("notify_highlights")]
    public int NotifyHighlights { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }
}

public class ActivityStat
{
    [JsonPropertyName("application_id")]
    public string ApplicationId { get; set; } = "";

    [JsonPropertyName("last_played_at")]
    public string? LastPlayedAt { get; set; }

    [JsonPropertyName("first_played_at")]
    public string? FirstPlayedAt { get; set; }

    [JsonPropertyName("total_duration")]
    public int TotalDuration { get; set; }

    [JsonPropertyName("total_discord_sku_duration")]
    public int TotalDiscordSkuDuration { get; set; }
}

public class UserProfileMetadata
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("boosting_started_at")]
    public string? BoostingStartedAt { get; set; }

    [JsonPropertyName("premium_started_at")]
    public string? PremiumStartedAt { get; set; }

    [JsonPropertyName("legacy_username")]
    public string? LegacyUsername { get; set; }
}
