using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MidStateShuttleService.Models
{
    public enum SchoolTerm
    {
        [Display(Name = "Spring")]
        Spring,

        [Display(Name = "Summer")]
        Summer,

        [Display(Name = "Fall")]
        Fall,

        [Display(Name = "Winter Interim")]
        Winterim
    }

    public enum RequestStatus
    {
        [Display(Name = "Pending")]
        Pending,

        [Display(Name = "Approved")]
        Approved,

        [Display(Name = "Denied")]
        Denied,

        [Display(Name = "Cancelled")]
        Cancelled
    }

    // !!**** Temporarily disabled validations, to be addressed in the next sprint. ****!!//
    [Table("Registration")]
    public partial class RegisterModel
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression("^[A-Za-z\\s'-]+$", ErrorMessage = "Must contain only letters, spaces, dashes, or apostrophes.")]
        [StringLength(60)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression("^[A-Za-z\\s'-]+$", ErrorMessage = "Must contain only letters, spaces, dashes, or apostrophes.")]
        [StringLength(60)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Must be 10 digits")]
        [StringLength(10)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100)]
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

        public string? ContactPreference { get; set; }

        [Required]
        public bool AgreeTerms { get; set; } = false;//  true/false for agreement

        public IEnumerable<SelectListItem>? LocationNames { get; set; }

        // Add new properties for route details
        public string? SelectedRouteDetail { get; set; }
        public string? ReturnSelectedRouteDetail { get; set; }

        // New property for selecting days of the week
        public List<string>? SelectedDaysOfWeek { get; set; } = new List<string>();

        public DateOnly? FirstDayExpectingToRide { get; set; }

        public TimeOnly? MustArriveTime { get; set; }

        public TimeOnly? CanLeaveTime { get; set; }

        public bool IsActive { get; set; }
        
        /// <summary>
        /// The student ID of the student associated with the registration.
        /// </summary>
        [StringLength(25)]
        //[RegularExpression(@"^\d{8}$", ErrorMessage = "The StudentID must be exactly 8 digits.")]
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

        [Required(ErrorMessage = "Term is required.")]
        public SchoolTerm? Term { get; set; }

        public int? ReturnPickUpLocationId { get; set; }

        public int? ReturnDropOffLocationId { get; set; }

        public string? SpecialRequestDescription { get; set; }

        public int? RouteId { get; set; }
        public int? ReturnRouteId { get; set; }

        public RequestStatus? RequestStatus { get; set; }

        [ForeignKey(nameof(RouteId))]
        [NotMapped]
        public virtual Routes? Route { get; set; }

        [ForeignKey(nameof(ReturnRouteId))]
        [NotMapped]
        public virtual Routes? ReturnRoute { get; set; }

        [NotMapped]
        public List<Routes> Routes { get; set; } = new List<Routes>();
    }
}
