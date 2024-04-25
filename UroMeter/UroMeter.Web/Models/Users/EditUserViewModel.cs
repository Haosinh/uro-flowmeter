namespace UroMeter.Web.Models.Users;

public class EditUserViewModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateOnly BirthDay { get; set; }

    public string Phone { get; set; }
}