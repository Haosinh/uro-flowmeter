using System.Text.Json.Serialization;

namespace UroMeter.Web.Models.MedicalRecord;

public class MedicalRecordDataDto
{
    [JsonPropertyName("x")]
    public int TimeInMilisecond { get; set; }

    [JsonPropertyName("y")]
    public int VolumnInMililiter { get; set; }
}