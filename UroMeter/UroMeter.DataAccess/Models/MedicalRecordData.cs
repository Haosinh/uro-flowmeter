namespace UroMeter.DataAccess.Models;

public class MedicalRecordData
{
    public int Id { get; set; }

    public int TimeInMilisecond { get; set; }

    public int VolumnInMililiter { get; set; }

    public int MedicalRecordId { get; set; }

    public MedicalRecord MedicalRecord { get; set; }
}