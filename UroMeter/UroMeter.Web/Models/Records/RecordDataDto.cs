﻿using System.Text.Json.Serialization;

namespace UroMeter.Web.Models.Records;

public class RecordDataDto
{
    [JsonPropertyName("x")]
    public DateTimeOffset RecordAt { get; set; }

    [JsonPropertyName("y")]
    public double Volume { get; set; }
}
