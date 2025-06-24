using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MidStateShuttleService.Models
{

    // !!**** Temporarily disabled validations, to be addressed in the next sprint. ****!!//
    [Table("Registration")]
    public partial class RegisterModel
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression("^[A-Za-z\\s'-]+$", ErrorMessage = "Must contain only letters, spaces, dashes, or apostrophes.")]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression("^[A-Za-z\\s'-]+$", ErrorMessage = "Must contain only letters, spaces, dashes, or apostrophes.")]
        [StringLength(20)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Must be 10 digits")]
        [StringLength(10)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(40)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Trip Type is required")]
        [StringLength(10)]
        public string TripType { get; set; }// This could be a dropdown in the UI linked to Types available

        //[Required(ErrorMessage = "Pick Up Location is required")]
        //public string PickUpLocation { get; set; }
        public int? PickUpLocationID { get; set; }

        //[Required(ErrorMessage = "Drop Off Location is required")]
        public int? DropOffLocationID { get; set; }

        //[StringLength(300, ErrorMessage = "Need transportation cannot exceed 300 characters")]
        public string? NeedTransportation { get; set; }

        //[Required(ErrorMessage = "Special request is required")]
        public bool? SpecialRequest { get; set; } = false; // Assuming this is mandatory for registration

        //[StringLength(300, ErrorMessage = "Which Friday cannot exceed 300 characters")]
        public string? WhichFriday { get; set; }

        //[Required(ErrorMessage = "Friday Trip Type is required")]
        public string? FridayTripType { get; set; }

        public string? ContactPreference { get; set; }

        [Required]
        public bool AgreeTerms { get; set; } = false;//  true/false for agreement

        [Required]
        public bool? FridayAgreeTerms { get; set; } = false;//  true/false for agreement

        public IEnumerable<SelectListItem>? LocationNames { get; set; }

        // Add new properties for route details
        public string? SelectedRouteDetail { get; set; }
        public string? ReturnSelectedRouteDetail { get; set; }

        // New property for selecting days of the week
        public List<string>? SelectedDaysOfWeek { get; set; } = new List<string>();

        public DateOnly? FirstDayExpectingToRide { get; set; }

        public TimeOnly? MustArriveTime { get; set; }

        public TimeOnly? CanLeaveTime { get; set; }


        public TimeOnly? FridayMustArriveTime { get; set; }

        public TimeOnly? FridayCanLeaveTime { get; set; }

        public string? SpecialPickUpLocation { get; set; }

        public string? SpecialDropOffLocation { get; set; }

        //[Required(ErrorMessage = "Pick Up Location is required")]
        public int? FridayPickUpLocationID { get; set; }

        //[Required(ErrorMessage = "Drop Off Location is required")]
        public int? FridayDropOffLocationID { get; set; }

        public bool IsActive { get; set; }
        
        /// <summary>
        /// The student ID of the student associated with the registration.
        /// </summary>
        [StringLength(25)]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "The StudentID must be exactly 8 digits.")]
        public string? StudentId {get; set; }

        /// <summary>
        /// The IP address of the device that created the record.
        /// </summary>
        [StringLength(50)]
        public string? DeviceIpAddress { get; set; }

        /// <summary>
        /// The date and time the record was created.
        /// </summary>
        public DateTime? InsertDateTime { get; set; }

        /// <summary>
        /// The date and time the record was last edited.
        /// </summary>
        public DateTime? EditDateTime { get; set; }
    }

}
