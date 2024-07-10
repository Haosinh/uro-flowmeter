namespace UroMeter.DataAccess.Models;

public class Record
{
    public int Id { get; set; }

    public DateTimeOffset CheckUpAt { get; set; } = DateTime.Now;

    public DateTimeOffset RecordAt { get; set; }

    public bool Finished { get; set; } = false;

    public int PatientId { get; set; }

    public User Patient { get; set; }
}
