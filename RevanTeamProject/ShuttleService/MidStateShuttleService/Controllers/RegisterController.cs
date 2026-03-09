using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MidStateShuttleService.Migrations;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
using MidStateShuttleService.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace MidStateShuttleService.Controllers
{
    public class RegisterController : Controller
    {
        private readonly EmailServices _emailServices;

        private readonly ApplicationDbContext _context;

        private readonly ILogger<RegisterController> _logger;

        // Inject ApplicationDbContext into the controller constructor
        public RegisterController(ApplicationDbContext context, EmailServices emailServices, ILogger<RegisterController> logger)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            _emailServices = emailServices;
            _logger = logger;
        }

        //overload method for default
        private List<SelectListItem> GetSchoolTermSelectList()
        {
            return GetSchoolTermSelectList(false);
        }

        /// <summary>
        /// Returns the list of Terms
        /// </summary>
        /// <param name="getSummer"></param>
        /// <returns></returns>
        private List<SelectListItem> GetSchoolTermSelectList(bool isSpecial)
        {
            var terms = Enum.GetValues(typeof(SchoolTerm))
                .Cast<SchoolTerm>();

            if (!isSpecial)
            {
                terms = terms.Where(t => t != SchoolTerm.Summer && t != SchoolTerm.Other);
            }

            return terms
                .Select(term => new SelectListItem
                {
                    Text = GetEnumDisplayName(term),
                    Value = term.ToString()
                })
                .ToList();
        }

        private string GetEnumDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName() ?? enumValue.ToString();
        }

        /// <summary>
        /// Index is the form to create a registration.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            LocationServices ls = new LocationServices(_context);

            string email = "";
            string phone = "";
            string fullName = "";
            string studentId = "";

            try
            {
                var oidClaim = User.FindFirst("oid")
                ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                string userId = "";
                if (oidClaim != null)
                {
                    userId = oidClaim?.Value;
                }

                var dbUser = _context.Users
                .FirstOrDefault(u => u.AzureAdObjectId == userId);

                if (dbUser != null)
                {
                    phone = dbUser.PhoneNumber;
                    fullName = dbUser.FirstName + " " + dbUser.LastName;
                    studentId = dbUser.StudentId;
                }

                email = User.FindFirst("email")?.Value
                            ?? User.FindFirst("preferred_username")?.Value
                            ?? User.Identity?.Name
                            ?? "";
            }
            catch (Exception ex)
            {
                email = "";
                phone = "";
                fullName = "";
                studentId = "";
            }

            var model = new RegisterModel();
            model.LocationNames = ls.GetLocationNames();

            model.Phone = phone;
            model.Email = email;
            model.Name = fullName;
            model.StudentId = studentId;

            //set trip type up for now, its a legacy feature
            model.TripType = "N/A";

            model.TimeOptions = GetTimeSelectList();
            ViewBag.Terms = GetSchoolTermSelectList();
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Completed the backend logic for a registration form submission
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            LocationServices ls = new LocationServices(_context);
            RegisterServices rs = new RegisterServices(_context);


            // Repopulate LocationNames for the model in case of return to View due to invalid model state or any error.
            model.LocationNames = ls.GetLocationNames();

            if (ModelState.IsValid)
            {
                var oidClaim = User.FindFirst("oid")
                ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                string userId = oidClaim?.Value;

                var dbUser = _context.Users
                .FirstOrDefault(u => u.AzureAdObjectId == userId);

                model.IsActive = true; // Set IsActive to true
                model.DeviceIpAddress = model.DeviceIpAddress ?? "Unknown";
                model.InsertDateTime = DateTime.Now;

                if (dbUser != null)
                {
                    model.UserId = dbUser.Id;
                }

                if (rs.AddEntity(model))
                {
                    // Increment the registration count in the session
                    int registrationCount = HttpContext.Session.GetInt32("RegistrationCount") ?? 0;
                    registrationCount++;

                    HttpContext.Session.SetInt32("RegistrationCount", registrationCount);

                    string emailBody = ""; 
                    
                    if (model.isCustom)
                    {
                        emailBody = BuildEmailForSpecialRegisterSubmit(model.RegistrationId);
                    }
                    else
                    {
                        emailBody = BuildEmailForRegisterSubmit(model.RegistrationId);
                    }

                    _emailServices.SendEmailToAdmin(
                        "MSTC Shuttle Service Request Confirmation",
                        emailBody,
                        isHtml: true
                    );

                    TempData["Success"] = "Registration created successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Something went wrong.";
                }
            }

            ViewBag.Terms = GetSchoolTermSelectList();

            return View("Index", model);
        }

        /// <summary>
        /// This is the only Controller action to use view models
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public IActionResult ViewRegistrations()
        {
            var registrations = new RegisterServices(_context).GetViewModels();

            return PartialView("~/Views/Register/_ViewRegistrations.cshtml", registrations);
        }

        // Displays the full breakdown of a single registration
        [Authorize(Roles = "Admin")]
        public IActionResult Details(int registrationId)
        {
            var registration = _context.RegisterModels
            .Include(r => r.DaySchedules)
                .ThenInclude(d => d.Rides)
                    .ThenInclude(r => r.Route)
            .FirstOrDefault(r => r.RegistrationId == registrationId);

            if (registration == null)
                return NotFound();

            ViewBag.Terms = GetSchoolTermSelectList();

            registration.TimeOptions = GetTimeSelectList();

            registration.LocationNames = _context.Locations
                .Select(l => new SelectListItem
                {
                    Value = l.LocationId.ToString(),
                    Text = l.Name
                })
                .ToList();

            var routesByPickDrop = new Dictionary<(int pickup, int dropoff), List<SelectListItem>>();

            foreach (var pickup in registration.LocationNames.Select(l => int.Parse(l.Value)))
            {
                foreach (var drop in registration.LocationNames.Select(l => int.Parse(l.Value)))
                {
                    routesByPickDrop[(pickup, drop)] = _context.Routes
                        .Where(r => r.PickUpLocationID == pickup && r.DropOffLocationID == drop)
                        .Select(r => new SelectListItem
                        {
                            Value = r.RouteID.ToString(),
                            Text = FormatTime(r.PickUpTime) + " > " + FormatTime(r.DropOffTime)
                        })
                        .ToList();
                }
            }
            ViewBag.RoutesByPickDrop = routesByPickDrop;

            return View(registration);
        }

        private static string FormatTime(TimeSpan? time)
        {
            if (time == null)
                return "";

            return time.Value.ToString(@"h\:mm");
        }

        //displays the details for special request/registrations
        [Authorize(Roles = "Admin")]
        public IActionResult SpecialDetails(int registrationId)
        {
            var model = _context.RegisterModels
                .FirstOrDefault(r => r.RegistrationId == registrationId);

            if (model == null)
                return NotFound();

            ViewBag.Terms = GetSchoolTermSelectList(true);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult EditSave(RegisterModel model)
        {
            LocationServices ls = new LocationServices(_context);

            if (!ModelState.IsValid)
            {
                ViewBag.Terms = GetSchoolTermSelectList();
                model.LocationNames = ls.GetLocationNames();
                model.TimeOptions = GetTimeSelectList();

                return View("Details", model);
            }

            var existing = _context.RegisterModels
                .Include(r => r.DaySchedules)
                    .ThenInclude(d => d.Rides)
                .FirstOrDefault(r => r.RegistrationId == model.RegistrationId);

            if (existing == null)
                return NotFound();

            // Update Registration fields
            existing.Term = model.Term;
            existing.LengthOfRequest = model.LengthOfRequest;
            existing.AgreeTerms = model.AgreeTerms;
            existing.IsAdult = model.IsAdult;
            existing.Email = model.Email;
            existing.Phone = model.Phone;
            existing.StudentId = model.StudentId;
            existing.Name = model.Name;

            // Update DaySchedules and Rides
            for (int i = 0; i < existing.DaySchedules.Count; i++)
            {
                var existingDay = existing.DaySchedules[i];
                var modelDay = model.DaySchedules[i];

                existingDay.WeekDay = modelDay.WeekDay;

                for (int j = 0; j < existingDay.Rides.Count; j++)
                {
                    var existingRide = existingDay.Rides[j];
                    var modelRide = modelDay.Rides[j];

                    existingRide.PickUpLocationID = modelRide.PickUpLocationID;
                    existingRide.DropOffLocationID = modelRide.DropOffLocationID;
                    existingRide.DropOffTime = modelRide.DropOffTime;

                    existingRide.RouteId = modelRide.RouteId;
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Details",
                new { registrationId = existing.RegistrationId });
        }

        /// <summary>
        /// Editing a Special Request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult SpecialEditSave(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Terms = GetSchoolTermSelectList(true);
                return View("SpecialDetails", model);
            }

            var existing = _context.RegisterModels
                .FirstOrDefault(r => r.RegistrationId == model.RegistrationId);

            if (existing == null)
                return NotFound();

            // Update only special request fields
            existing.Term = model.Term;
            existing.customDate = model.customDate;
            existing.customTime1 = model.customTime1;
            existing.customTime2 = model.customTime2;
            existing.customMessage = model.customMessage;
            existing.AgreeTerms = model.AgreeTerms;
            existing.IsAdult = model.IsAdult;
            existing.Email = model.Email;
            existing.Phone = model.Phone;

            _context.SaveChanges();

            return RedirectToAction("SpecialDetails",
                new { registrationId = existing.RegistrationId });
        }

        private List<SelectListItem> GetTimeSelectList()
        {
            var times = new List<SelectListItem>();

            var start = new TimeOnly(7, 30);
            var end = new TimeOnly(16, 0);

            for (var time = start; time <= end; time = time.AddMinutes(30))
            {
                times.Add(new SelectListItem
                {
                    Value = time.ToString("h:mm tt"),
                    Text = time.ToString("h:mm tt")
                });
            }

            return times;
        }

        [HttpGet]
        public ActionResult SpecialRequest()
        {
            LocationServices ls = new LocationServices(_context);

            string email = "";
            string phone = "";
            string fullName = "";
            string studentId = "";

            try
            {
                var oidClaim = User.FindFirst("oid")
                ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                string userId = "";
                if (oidClaim != null)
                {
                    userId = oidClaim?.Value;
                }

                var dbUser = _context.Users
                .FirstOrDefault(u => u.AzureAdObjectId == userId);

                if (dbUser != null)
                {
                    phone = dbUser.PhoneNumber;
                    fullName = dbUser.FirstName + " " + dbUser.LastName;
                    studentId = dbUser.StudentId;
                }

                email = User.FindFirst("email")?.Value
                            ?? User.FindFirst("preferred_username")?.Value
                            ?? User.Identity?.Name
                            ?? "";
            }
            catch (Exception ex)
            {
                email = "";
                phone = "";
                fullName = "";
                studentId = "";
            }

            var model = new RegisterModel();
            model.isCustom = true;
            model.LocationNames = ls.GetLocationNames();
            model.Email = email;
            model.Phone = phone;
            model.Name = fullName;
            model.StudentId = studentId;
            ViewBag.Terms = GetSchoolTermSelectList(true);
            return View("SpecialRequest", model);
        }

        [Authorize]
        public ActionResult RegisterConfirmation(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                return View(model);
            }

            return View("Index", model);
        }


        //retrieves route options based on selected pick-up and drop-off locations from a database and returns them as JSON.
        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetRoutes(int pickUpLocationId, int dropOffLocationId)
        {
            RouteServices rs = new RouteServices(_context);
            // This call will now also check the IsActive property of each route
            var routesList = rs.GetRoutesByLocations(pickUpLocationId, dropOffLocationId)
                               .Where(route => route.IsActive).ToList();
            LocationServices ls = new LocationServices(_context);

            var formattedRoutesList = new List<object>();
            foreach (var r in routesList)
            {
                if (r.AdditionalDetails != null)
                    formattedRoutesList.Add(new
                    {
                        r.RouteID,
                        Detail = $"Leave {ls.getLocationNameById(r.PickUpLocationID)} at {r.ToStringPickUpTime()} ({r.AdditionalDetails}), Arrive at {ls.getLocationNameById(r.DropOffLocationID)} at {r.ToStringDropOffTime()}"
                    });
                else
                    formattedRoutesList.Add(new
                    {
                        r.RouteID,
                        Detail = $"Leave {ls.getLocationNameById(r.PickUpLocationID)} at {r.ToStringPickUpTime()}, Arrive at {ls.getLocationNameById(r.DropOffLocationID)} at {r.ToStringDropOffTime()}"
                    });
            }

            return Json(formattedRoutesList);
        }

        //THIS IS UNUSED
        public ActionResult Edit(int id)
        {
            LocationServices ls = new LocationServices(_context);
            RouteServices rs = new RouteServices(_context);

            // Retrieve the student to be edited from the database
            var student = _context.RegisterModels.Find(id);

            if (student == null)
            {
                return NotFound(); // Or handle the case where the student is not found
            }

            // Retrieve the days of the week selected for the student
            var selectedDaysOfWeek = _context.RegisterModels
                                              .Where(s => s.RegistrationId == id)
                                              .Select(s => s.SelectedDaysOfWeek)
                                              .FirstOrDefault();

            // Pass the selected days of the week to the view
            ViewBag.SelectedDaysOfWeek = selectedDaysOfWeek;

            ViewBag.RouteList = rs.GetAllEntities();

            ViewBag.SelectedPickupRoute = student.SelectedRouteDetail;
            ViewBag.SelectedReturnRoute = student.ReturnSelectedRouteDetail;
            ViewBag.Terms = GetSchoolTermSelectList();

            // Return the location names for each route
            foreach (Routes route in ViewBag.RouteList)
            {
                route.PickUpLocation = ls.GetEntityById(route.PickUpLocationID);
                route.DropOffLocation = ls.GetEntityById(route.DropOffLocationID);
            }

            return View(student);
        }

        //THIS IS UNUSED
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, RegisterModel student)
        {
            if (id != student.RegistrationId)
            {
                return BadRequest(); // Or handle the case where IDs do not match
            }

            // Make sure the return route is null if the student selected one way
            if (student.TripType == "OneWay")
            {
                student.ReturnSelectedRouteDetail = null;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Terms = GetSchoolTermSelectList();
                return View(student); // Return the view with validation errors
            }

            try
            {
                // Update the student in the database
                student.IsActive = true; // Set IsActive to true
                _context.Update(student);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "The student has been successfully updated!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                //LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context); // Log SQL exception
                _logger.LogError(ex, "An error occurred while updating student.");
                ModelState.AddModelError("", "An unexpected error occurred, please try again.");

                ViewBag.Terms = GetSchoolTermSelectList();

                return View(student); // Return the view with an error message
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ArchiveRegistration(int id)
        {
            var registration = _context.RegisterModels.FirstOrDefault(r => r.RegistrationId == id);
            if (registration == null)
                return NotFound();

            if (!User.IsInRole("Admin"))
                return Forbid();

            registration.IsArchived = true;
            _context.SaveChanges();

            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// Email content to generate for a registration (requested ride) confirmation email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GenerateRegistrationEmailBody(RegisterModel model)
        {
            try
            {
                string initialRoute = "Unknown";

                // Check if the pick-up and drop-off locations are valid
                if (model.PickUpLocationID == null || model.DropOffLocationID == null)
                {
                    return "Invalid pick-up or drop-off location";
                }
                else
                {
                    ActionResult actionResult = null;

                    if (model.SpecialRequest != null)
                    {
                        if (model.SpecialRequest.Value == true)
                        {
                            return SpecialRequestRoute(model);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Could not create special request. Check your request and try again.");
                        }
                    }

                    model.SpecialRequest = false; // Default to false if SpecialRequest is null
                    actionResult = GetRoutes(model.PickUpLocationID.Value, model.DropOffLocationID.Value);
                    initialRoute = this.ParseInitialResult(actionResult, initialRoute);

                    return "";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                //LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while generating request email body.");
                return "An error occurred while generating request email body.";
            }
        }

        /// <summary>
        /// Build the route for special requests in the email.
        /// </summary>
        /// <param name="model">Model of the request.</param>
        /// <returns>The email confirmation body for a special request.</returns>
        private string SpecialRequestRoute(RegisterModel model)
        {
            string initialRoute = "Unknown";
            string pickupLocationName = "Unknown";
            string dropoffLocationName = "Unknown";
            string pickupTime = TimeOnly.MaxValue.ToShortTimeString();
            string dropoffTime = TimeOnly.MaxValue.ToShortTimeString();
            List<object> formattedRoutesList = new List<object>();
            LocationServices ls = new LocationServices(_context);

            if (model.PickUpLocationID.HasValue)
            {
                var x = ls.getLocationNameById(model.PickUpLocationID.Value);
                if (x.ToLower() == "other")
                {
                    // Other routes
                    pickupLocationName = model.SpecialPickUpLocation;
                    pickupTime = ToStringRideTimes(model.MustArriveTime.Value);
                }
                else
                {
                    // Standard routes
                    //pickupLocationName = model.PickUpLocationID.HasValue ? ls.getLocationNameById(model.PickUpLocationID.Value) : "Unknown";
                    //pickupTime = ToStringRideTimes(model.MustArriveTime.Value);

                    var routeInfo = GetRouteInfo(model.PickUpLocationID.Value, model.DropOffLocationID.Value);

                    pickupLocationName = "";
                }
            }

            if (model.DropOffLocationID.HasValue)
            {
                var x = ls.getLocationNameById(model.DropOffLocationID.Value);
                if (x.ToLower() == "other")
                {
                    // Other routes
                    dropoffLocationName = model.SpecialDropOffLocation;
                    dropoffTime = ToStringRideTimes(model.CanLeaveTime.Value);
                }
                else
                {
                    // Standard routes
                    dropoffLocationName = model.DropOffLocationID.HasValue ? ls.getLocationNameById(model.DropOffLocationID.Value) : "Unknown";
                    dropoffTime = ToStringRideTimes(model.CanLeaveTime.Value);
                }
            }

            formattedRoutesList.Add(new
            {
                RouteID = "other",
                Detail = $"Leave {dropoffLocationName} at {pickupTime}, Arrive at {dropoffLocationName} at {dropoffTime}"
            });

            JsonResult finalList = Json(formattedRoutesList);

            initialRoute = ParseInitialResult(finalList, initialRoute);

            return "";
        }

        private JsonResult GetRouteInfo(int pickUpLocationId, int dropOffLocationId)
        {
            RouteServices rs = new RouteServices(_context);
            var routesList = rs.GetRoutesByLocations(pickUpLocationId, dropOffLocationId).Where(route => route.IsActive).ToList();

            LocationServices ls = new LocationServices(_context);

            var formattedRoutesList = new List<object>();
            foreach (var r in routesList)
            {
                formattedRoutesList.Add(new
                {
                    r.RouteID,
                    PickupLocation = ls.getLocationNameById(r.PickUpLocationID),
                    PickupTime = r.ToStringPickUpTime(),
                    DropOffLocation = ls.getLocationNameById(r.DropOffLocationID),
                    DropOffTime = r.ToStringDropOffTime(),
                    AdditionalDetails = r.AdditionalDetails != null ? r.AdditionalDetails : null
                });
            }

            return Json(formattedRoutesList);
        }

        /// <summary>
        /// Convert ride times to a string format for display in the email.
        /// </summary>
        /// <param name="time">Time to return as a string.</param>
        /// <returns>A string formatted time.</returns>
        private string ToStringRideTimes(TimeOnly? time)
        {
            if (time.HasValue)
            {
                return time.Value.ToString("hh:mm tt");
            }
            return "N/A";
        }

        /// <summary>
        /// Parse the result of the routes to build the initial route string.
        /// </summary>
        /// <param name="actionResult">ActionResult object to parse</param>
        /// <param name="initialRoute">The initialized route to modify.</param>
        /// <returns>The final route to be displayed in email.</returns>
        private string ParseInitialResult(ActionResult actionResult, string initialRoute)
        {
            if (actionResult is JsonResult jsonResult)
            {
                string jsonString = JsonSerializer.Serialize(jsonResult.Value);

                // Parse the JSON string
                using JsonDocument doc = JsonDocument.Parse(jsonString);

                // Assuming the first route in the list is required
                initialRoute = doc.RootElement[0].GetProperty("Detail").GetString();
            }

            return initialRoute;
        }

        /// <summary>
        /// Builds the email confirmation body for a registration request.
        /// </summary>
        private string BuildEmailForRegisterSubmit(int id)
        {
            var registration = _context.RegisterModels
            .Include(r => r.DaySchedules)
                .ThenInclude(d => d.Rides)
                    .ThenInclude(r => r.PickUpLocation)
            .Include(r => r.DaySchedules)
                .ThenInclude(d => d.Rides)
                    .ThenInclude(r => r.DropOffLocation)
            .FirstOrDefault(r => r.RegistrationId == id);

            if (registration == null)
                return "<p>Registration not found.</p>";

            string isAdultText = registration.IsAdult
                ? "The Rider is an Adult"
                : "The Rider is NOT an Adult";

            string sId = "N/A";
            if (registration.StudentId != "")
            {
                sId = registration.StudentId;
            }

            string html = $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <h2>MSTC Shuttle Service Request Confirmation</h2>
            <h3><strong>Student Name:</strong> {registration.Name}</h3>
            <h3><strong>Student Id:</strong> {sId}</h3>
            <p><strong>Email:</strong> {registration.Email}</p>
            <p><strong>Phone:</strong> {registration.Phone}</p>
            <p><strong>Status:</strong> {isAdultText}</p>
            <hr/>
            ";

            if (registration.DaySchedules != null)
            {
                foreach (var day in registration.DaySchedules)
                {
                    html += $"<h3>Day: {day.WeekDay}</h3>";

                    if (day.Rides != null && day.Rides.Any())
                    {
                        html += @"
                    <table border='1' cellpadding='6' cellspacing='0' style='border-collapse: collapse; margin-bottom:15px;'>
                        <tr style='background-color:#f2f2f2;'>
                            <th>Pick-Up</th>
                            <th>Drop-Off</th>
                            <th>Time</th>
                        </tr>
                        ";

                        foreach (var ride in day.Rides)
                        {
                            html += $@"
                        <tr>
                            <td>{ride.PickUpLocation?.Name ?? "Unknown"}</td>
                            <td>{ride.DropOffLocation?.Name ?? "Unknown"}</td>
                            <td>{ride.DropOffTime}</td>
                        </tr>
                    ";
                        }

                        html += "</table>";
                    }
                    else
                    {
                        html += "<p>No rides scheduled for this day.</p>";
                    }
                }
            }

            html += @"
            <hr/>
            <p>This request will be reviewed by the shuttle program and is not guaranteed.</p>
        </body>
        </html>
            ";

            return html;
        }

        /// <summary>
        /// Builds the email confirmation body for a SPECIAL registration request.
        /// </summary>
        private string BuildEmailForSpecialRegisterSubmit(int id)
        {
            var registration = _context.RegisterModels
                .FirstOrDefault(r => r.RegistrationId == id);

            if (registration == null)
                return "<p>Registration not found.</p>";

            string isAdultText = registration.IsAdult
                ? "The Rider is an Adult"
                : "The Rider is NOT an Adult";

            string dateText = registration.customDate.HasValue
                ? registration.customDate.Value.ToString("MMMM dd, yyyy")
                : "Not Provided";

            string time1Text = registration.customTime1.HasValue
                ? registration.customTime1.Value.ToString("hh:mm tt")
                : "Not Provided";

            string time2Text = registration.customTime2.HasValue
                ? registration.customTime2.Value.ToString("hh:mm tt")
                : null;

            string sId = "N/A";
            if (registration.StudentId != "")
            {
                sId = registration.StudentId;
            }

            string html = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>MSTC Shuttle Service SPECIAL Request Confirmation</h2>

                    <h3><strong>Student Name:</strong> {registration.Name}</h3>
                    <h3><strong>Student Id:</strong> {sId}</h3>
                    <p><strong>Email:</strong> {registration.Email}</p>
                    <p><strong>Phone:</strong> {registration.Phone}</p>
                    <p><strong>Status:</strong> {isAdultText}</p>

                    <hr/>

                    <h3>Special Ride Details</h3>

                    <p><strong>Date:</strong> {dateText}</p>
                    <p><strong>Time:</strong> {time1Text}</p>
                ";

            if (!string.IsNullOrEmpty(time2Text))
            {
                html += $"<p><strong>Return Time:</strong> {time2Text}</p>";
            }

            if (!string.IsNullOrWhiteSpace(registration.customMessage))
            {
                html += $@"
                    <hr/>
                    <h3>Additional Details</h3>
                    <p>{registration.customMessage}</p>
                    ";
                        }

                        html += @"
                    <hr/>
                </body>
                </html>
                ";

            return html;
        }
    }
}

