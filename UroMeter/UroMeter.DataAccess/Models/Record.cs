namespace UroMeter.DataAccess.Models;

public class Record
{
    public int Id { get; set; }

    public DateTime CheckUpAt { get; set; }

    public int PatientId { get; set; }

    public User Patient { get; set; }
}
