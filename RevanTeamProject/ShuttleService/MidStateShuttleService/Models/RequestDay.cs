using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MidStateShuttleService.Enums;
using MidStateShuttleService.Models;

[Table("RequestDay")]
public class RequestDay
{
    [Key]
    public int RequestDayId { get; set; }

    [Required]
    public WeekDay WeekDay { get; set; }

    // FK
    [Required]
    public int RegistrationId { get; set; }

    [ForeignKey(nameof(RegistrationId))]
    public RegisterModel? Registration { get; set; }

    // rides
    public List<Ride> Rides { get; set; } = new();
}