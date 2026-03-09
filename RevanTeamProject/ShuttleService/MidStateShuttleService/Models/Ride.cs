using MidStateShuttleService.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Ride")]
public class Ride
{
    [Key]
    public int RideId { get; set; }

    public int RequestDayId { get; set; }

    [ForeignKey(nameof(RequestDayId))]
    public RequestDay? RequestDay { get; set; }

    // Locations
    [Required]
    public int PickUpLocationID { get; set; }

    [ForeignKey(nameof(PickUpLocationID))]
    public Location? PickUpLocation { get; set; }


    [Required]
    public int DropOffLocationID { get; set; }

    [ForeignKey(nameof(DropOffLocationID))]
    public Location? DropOffLocation { get; set; }


    // Time they must arrive
    public TimeOnly? DropOffTime { get; set; }

    public int? RouteId { get; set; }

    public Routes? Route { get; set; }
}