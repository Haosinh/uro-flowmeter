namespace UroMeter.Web.Mqtt.Dtos;

public class DataRecordDto
{
    public DataCommand Command { get; set; }

    public DateTimeOffset? RecordAt { get; set; }

    public double? Volume { get; set; }

    public bool IsValid()
    {
        if (Command == DataCommand.INVALID)
        {
            return false;
        }

        if (Command == DataCommand.RECORD)
        {
            return RecordAt is not null && Volume != null;
        }

        return true;
    }
}
