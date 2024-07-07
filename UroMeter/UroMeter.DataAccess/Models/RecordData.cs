namespace UroMeter.DataAccess.Models;

public class RecordData
{
    public int Id { get; set; }

    public long Time { get; set; }

    public double Volume { get; set; }

    public int RecordId { get; set; }

    public Record Record { get; set; }
}
