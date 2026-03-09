using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MidStateShuttleService.Models
{
    [Table("Route")]
    [Index("DropOffLocationID", Name = "IX_Route_DropOffLocationID")]
    [Index("PickUpLocationID", Name = "IX_Route_PickUpLocationID")]
    public class Routes
    {
        [Key]
        public int RouteID { get; set; }

        [Display(Name = "Pick Up Location")]
        [Required(ErrorMessage = "Please select a pick-up location.")]
        public int PickUpLocationID { get; set; }

        [Display(Name = "Drop Off Location")]
        [Required(ErrorMessage = "Please select a drop-off location.")]
        public int DropOffLocationID { get; set; }

        [Display(Name = "Pick Up Time")]
        [Required(ErrorMessage = "Please enter a valid pick-up time.")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = true)]
        [RegularExpression("^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Please enter a valid time.")]
        public TimeSpan? PickUpTime { get; set; }

        [Display(Name = "Drop Off Time")]
        [Required(ErrorMessage = "Please enter a valid drop-off time.")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = true)]
        [RegularExpression("^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Please enter a valid time.")]
        public TimeSpan? DropOffTime { get; set; }

        [Display(Name = "Additional Details")]
        [StringLength(500, ErrorMessage = "Additional details cannot exceed 500 characters.")]
        [RegularExpression("^[a-zA-Z0-9.,!?'\";:@#$%^&*()_+=\\-\\/]*$", ErrorMessage = "Additional details can only contain letters, numbers, and important special characters.")]
        public string? AdditionalDetails { get; set; }

        public int DriverId { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("PickUpLocationID")]
        public virtual Location PickUpLocation { get; set; }

        [ForeignKey("DropOffLocationID")]
        public virtual Location DropOffLocation { get; set; }

        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        [NotMapped]
        public string? RouteInfo;

        public string ToStringPickUp()
        {
            return PickUpLocation.Name;
        }

        public string ToStringDropOff()
        {
            return DropOffLocation.Name;
        }

        public string ToStringPickUpTime()
        {
            if (PickUpTime.HasValue)
            {
                if (DateTime.TryParse(PickUpTime.ToString(), out DateTime pickUpDateTime))
                {
                    return pickUpDateTime.ToString("hh:mm tt");
                }
                else
                {
                    // Handle the case where PickUpTime is not in the expected format
                    return "Invalid Time"; // Or any default value you prefer
                }
            }
            else
            {
                // Handle the case where PickUpTime is null
                return "N/A"; // Or any default value you prefer
            }
        }

        public string ToStringDropOffTime()
        {
            if (DropOffTime.HasValue)
            {
                if (DateTime.TryParse(DropOffTime.ToString(), out DateTime dropOffDateTime))
                {
                    return dropOffDateTime.ToString("hh:mm tt");
                }
                else
                {
                    // Handle the case where DropOffTime is not in the expected format
                    return "Invalid Time"; // Or any default value you prefer
                }
            }
            else
            {
                // Handle the case where DropOffTime is null
                return "N/A"; // Or any default value you prefer
            }
        }



    }
}
