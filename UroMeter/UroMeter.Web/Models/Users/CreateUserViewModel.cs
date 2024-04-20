using System.ComponentModel.DataAnnotations;

namespace UroMeter.Web.Models.Users;

public class CreateUserViewModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    public DateOnly BirthDay { get; set; }

    [Required]
    public string Phone { get; set; }
}