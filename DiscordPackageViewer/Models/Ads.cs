using System.Text.Json.Serialization;

namespace DiscordPackageViewer.Models;

public class AdTraits
{
    [JsonPropertyName("day_pt")]
    public string? DayPt { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("reg_country_code")]
    public string? RegCountryCode { get; set; }

    [JsonPropertyName("reg_region")]
    public string? RegRegion { get; set; }

    [JsonPropertyName("reg_region_code")]
    public string? RegRegionCode { get; set; }

    [JsonPropertyName("primary_platform_l30")]
    public string? PrimaryPlatformL30 { get; set; }

    [JsonPropertyName("age_group")]
    public string? AgeGroup { get; set; }

    [JsonPropertyName("is_underage")]
    public bool IsUnderage { get; set; }

    [JsonPropertyName("has_active_mobile_subscription")]
    public bool HasActiveMobileSubscription { get; set; }

    [JsonPropertyName("has_active_subscription")]
    public bool HasActiveSubscription { get; set; }

    [JsonPropertyName("subscription_premium_type")]
    public int SubscriptionPremiumType { get; set; }

    [JsonPropertyName("game_names_clean_l365")]
    public List<string> GameNamesL365 { get; set; } = [];

    [JsonPropertyName("game_names_clean_l90")]
    public List<string> GameNamesL90 { get; set; } = [];

    [JsonPropertyName("game_ids_l30")]
    public List<string> GameIdsL30 { get; set; } = [];

    [JsonPropertyName("game_ids_l90")]
    public List<string> GameIdsL90 { get; set; } = [];

    [JsonPropertyName("game_ids_l365")]
    public List<string> GameIdsL365 { get; set; } = [];

    [JsonPropertyName("game_ids_l730")]
    public List<string> GameIdsL730 { get; set; } = [];

    [JsonPropertyName("genre_names_l90")]
    public List<string> GenreNamesL90 { get; set; } = [];

    [JsonPropertyName("genre_ids_l90")]
    public List<string> GenreIdsL90 { get; set; } = [];

    [JsonPropertyName("theme_names_l90")]
    public List<string> ThemeNamesL90 { get; set; } = [];

    [JsonPropertyName("theme_ids_l90")]
    public List<string> ThemeIdsL90 { get; set; } = [];

    [JsonPropertyName("quest_history_enrolled")]
    public List<string> QuestHistoryEnrolled { get; set; } = [];

    [JsonPropertyName("quest_history_reward_claimed")]
    public List<string> QuestHistoryRewardClaimed { get; set; } = [];

    [JsonPropertyName("ml_genre_ids_v1")]
    public List<string> MlGenreIdsV1 { get; set; } = [];

    [JsonPropertyName("mobile_genre_names")]
    public List<string> MobileGenreNames { get; set; } = [];

    [JsonPropertyName("ml_mobile_genre_names_v1")]
    public List<string> MlMobileGenreNamesV1 { get; set; } = [];

    [JsonPropertyName("movie_and_tv_genre_names")]
    public List<string> MovieAndTvGenreNames { get; set; } = [];

    [JsonPropertyName("music_and_audio_genre_names")]
    public List<string> MusicAndAudioGenreNames { get; set; } = [];

    [JsonPropertyName("custom_audiences")]
    public List<string> CustomAudiences { get; set; } = [];
}
