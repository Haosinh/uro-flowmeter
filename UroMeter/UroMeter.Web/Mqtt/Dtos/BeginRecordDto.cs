namespace UroMeter.Web.Mqtt.Dtos;

public class BeginRecordDto
{
    public DataCommand Command { get; set; }

    public DateTimeOffset RecordAt { get; set; }
}
