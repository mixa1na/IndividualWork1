using System.Text.Json.Serialization;

namespace IndividualWork1.Models;

public class PipelineConfig
{
    [JsonPropertyName("pipeline")]
    public List<PipelineStage> Stages { get; set; } = new();
}

public class PipelineStage
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public string Arguments { get; set; } = string.Empty;

    [JsonPropertyName("stopOnFailure")]
    public bool StopOnFailure { get; set; } = true;
}