using System.Text.Json.Serialization;

namespace UroMeter.Web.Models.Records;

public class SpeedDataDto
{
    [JsonPropertyName("x")]
    public DateTimeOffset RecordAt { get; set; }

    [JsonPropertyName("y")]
    public double Speed { get; set; }
}
