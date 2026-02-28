using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordPackageViewer.Models;

/// <summary>
/// Generic envelope for all files in Account/user_data_exports/.
/// The records are kept as raw JsonElement arrays since each section has different columns.
/// </summary>
public class DataExportEnvelope
{
    [JsonPropertyName("section")]
    public string Section { get; set; } = "";

    [JsonPropertyName("generated_at")]
    public string GeneratedAt { get; set; } = "";

    [JsonPropertyName("record_count")]
    public int RecordCount { get; set; }

    [JsonPropertyName("metadata")]
    public ExportMetadata? Metadata { get; set; }

    [JsonPropertyName("records")]
    public List<JsonElement> Records { get; set; } = [];
}

public class ExportMetadata
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("schema_name")]
    public string SchemaName { get; set; } = "";

    [JsonPropertyName("schema_description")]
    public string? SchemaDescription { get; set; }

    [JsonPropertyName("columns")]
    public List<ExportColumn> Columns { get; set; } = [];

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class ExportColumn
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
