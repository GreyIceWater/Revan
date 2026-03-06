using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MidStateShuttleService.Models
{
    public enum SchoolTerm
    {
        [Display(Name = "Spring")]
        Spring,

        [Display(Name = "Fall")]
        Fall,

        [Display(Name = "Winter Interim")]
        Winterim,

        [Display(Name = "Summer")]
        Summer,

        [Display(Name = "Other")]
        Other
    }

    public enum LengthOfRequest
    {
        [Display(Name = "Full Semester")]
        FullSemester,

        [Display(Name = "First 8 Weeks")]
        FirstHalf,

        [Display(Name = "Last 8 Weeks")]
        SecondHalf
    }

    // !!**** Temporarily disabled validations, to be addressed in the next sprint. ****!!//
    [Table("Registration")]
    public partial class RegisterModel
    {
        [Key]
        public int RegistrationId { get; set; }

        [StringLength(10)]
        public string? TripType { get; set; }// This could be a dropdown in the UI linked to Types available

        //[Required(ErrorMessage = "Pick Up Location is required")]
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
        //Mid-State Student ID
        [RegularExpression(@"^$|^\d{8}$",
        ErrorMessage = "Student ID must be exactly 8 digits if provided.")]
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

        public bool IsAdult { get; set; }

        /// <summary>
        /// True if this request is a field trip.
        /// </summary>
        public bool IsFieldTrip { get; set; }

        /// <summary>
        /// True if this request is an internal inquiry.
        /// </summary>
        public bool IsInternalInquiry { get; set; }

        [Required(ErrorMessage = "Term is required.")]
        public SchoolTerm? Term { get; set; }

        /// <summary>
        /// Bool Variable for if the Ride is a "Special Request"
        /// </summary>
        public bool isCustom { get; set; }

        /// <summary>
        /// The name of the location for the rider to be picked up from if the rider is making a special request
        /// </summary>
        public string? customPickupLocation { get; set; }

        /// <summary>
        /// The name of the location for the rider to be dropped off from if the rider is making a special request
        /// </summary>
        public string? customDropoffLocation { get; set; }

        /// <summary>
        /// Special Field So Riders Creating a special request can enter any additional details
        /// </summary>
        public string? customMessage { get; set; }

        /// <summary>
        /// The date that the rider is going to be picked up from
        /// </summary>
        public DateOnly? customDate { get; set; }

        /// <summary>
        /// The time that the rider needs to be picked up at (if not a round trip this is the only one set)
        /// </summary>
        public TimeOnly? customTime1 { get; set; }

        /// <summary>
        /// The time that the rider needs to be picked up at during their second ride of a round trip
        /// </summary>
        public TimeOnly? customTime2 { get; set; }

        public User? User { get; set; }

        /// <summary>
        /// The user who created the request's id
        /// </summary>
        public int? UserId { get; set; }

        public List<RequestDay> DaySchedules { get; set; } = new();

        /// <summary>
        /// The enum for the length of a standard request (First 8 Weeks, Second 8 Weeks, Full Semester)
        /// </summary>
        public LengthOfRequest LengthOfRequest { get; set; }

        /// <summary>
        /// The options for time in the select
        /// </summary>
        [NotMapped]
        public List<SelectListItem>? TimeOptions { get; set; }

        public bool IsArchived { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [RegularExpression(@"^$|^\d{10}$",
        ErrorMessage = "Phone number must be exactly 10 digits if provided. (Only Numbers, No Spaces or Dashes)")]
        [Required(ErrorMessage = "Phone is required.")]
        public string Phone { get; set; }

        [Required]
        public string Name { get; set; }
    }

}
