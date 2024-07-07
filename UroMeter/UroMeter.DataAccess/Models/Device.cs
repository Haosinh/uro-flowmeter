using System.ComponentModel.DataAnnotations;

namespace UroMeter.DataAccess.Models;

public class Device
{
    [Key]
    [Required]
    required public string MacAddress { get; set; }

    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.Now;

    public int? PatientId { get; set; } = null;

    public User? Patient { get; set; } = null;
}
