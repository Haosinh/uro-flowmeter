namespace UroMeter.DataAccess.Models;

public class MedicalRecordData
{
    public int Id { get; set; }

    /// <summary>
    /// X axis.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Y axis.
    /// </summary>
    public double Height { get; set; }

    public int MedicalRecordId { get; set; }

    public MedicalRecord MedicalRecord { get; set; }
}