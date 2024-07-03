using System.ComponentModel.DataAnnotations;

namespace UroMeter.Web.Models.Users;

public class CreateUserViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required(ErrorMessage = "The Birthday field is required.")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly BirthDay { get; set; }

    [Required]
    public string Phone { get; set; }
}
