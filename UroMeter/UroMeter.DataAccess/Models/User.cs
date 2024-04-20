namespace UroMeter.DataAccess.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateOnly BirthDay { get; set; }

    public string Phone { get; set; }
}