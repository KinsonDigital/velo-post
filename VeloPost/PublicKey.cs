using System.Text.Json.Serialization;

public class PublicKey
{
    [JsonPropertyName("key_id")]
    public string KeyId { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}
