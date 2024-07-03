using System.ComponentModel.DataAnnotations;

namespace UroMeter.DataAccess.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly BirthDay { get; set; }

    public string Phone { get; set; }
}
