namespace UroMeter.DataAccess.Models;

public class RecordData
{
    public int Id { get; set; }

    public int TimeInMilisecond { get; set; }

    public int VolumnInMililiter { get; set; }

    public int MedicalRecordId { get; set; }

    public Record Record { get; set; }
}
