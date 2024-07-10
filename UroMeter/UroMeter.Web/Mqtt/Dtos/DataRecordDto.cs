namespace UroMeter.Web.Mqtt.Dtos;

public class DataRecordDto
{
    public DataCommand Command { get; set; }

    public long Time { get; set; }

    public double? Volume { get; set; }

    public bool IsValid()
    {
        if (Command == DataCommand.INVALID)
        {
            return false;
        }

        if (Command == DataCommand.RECORD)
        {
            return Volume != null;
        }

        return true;
    }
}
