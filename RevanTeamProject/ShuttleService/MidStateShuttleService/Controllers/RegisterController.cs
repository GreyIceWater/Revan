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

            var model = new RegisterModel();
            model.LocationNames = ls.GetLocationNames();

            model.Phone = phone;
            model.Email = email;
            model.Name = fullName;
            model.StudentId = studentId;

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

            model.TimeOptions = GetTimeSelectList();

            model.InsertDateTime = DateTime.UtcNow;

            // Repopulate LocationNames in case we return to the view
            model.LocationNames = ls.GetLocationNames();

            if (ModelState.IsValid)
            {
                // -------- VALIDATION SECTION --------

                // Ensure days are not repeated
                if (model.DaySchedules != null)
                {
                    var duplicateDays = model.DaySchedules
                        .GroupBy(d => d.WeekDay)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateDays.Any())
                    {
                        TempData["Error"] = "Each request day (Monday–Thursday) can only be selected once.";
                        ViewBag.Terms = GetSchoolTermSelectList();
                        return View("Index", model);
                    }
                }

                // Skip ride/day validation if custom
                if (!model.isCustom)
                {
                    // Ensure at least one ride exists
                    bool hasRide = model.DaySchedules != null &&
                                   model.DaySchedules.Any(d => d.Rides != null && d.Rides.Any());

                    if (!hasRide)
                    {
                        TempData["Error"] = "At least one ride must be added to submit a registration.";
                        ViewBag.Terms = GetSchoolTermSelectList();
                        return View("Index", model);
                    }

                    // Ensure no request day has zero rides
                    bool emptyDayExists = model.DaySchedules != null &&
                                          model.DaySchedules.Any(d => d.Rides == null || !d.Rides.Any());

                    if (emptyDayExists)
                    {
                        TempData["Error"] = "Every request day must contain at least one ride.";
                        ViewBag.Terms = GetSchoolTermSelectList();
                        return View("Index", model);
                    }
                }

                // Ensure each ride has either Route OR Time
                bool invalidRide = model.DaySchedules != null &&
                                   model.DaySchedules.Any(d =>
                                        d.Rides != null &&
                                        d.Rides.Any(r =>
                                            r.RouteId == null &&
                                            string.IsNullOrWhiteSpace(r.DropOffTime.ToString())));

                if (invalidRide)
                {
                    TempData["Error"] = "Each ride must have either a route selected or a drop-off time.";
                    ViewBag.Terms = GetSchoolTermSelectList();
                    return View("Index", model);
                }

                // -------- SAVE REGISTRATION --------

                if (rs.AddEntity(model))
                {
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

                    //Send notification for the admin page

                    Notification notif = new Notification();
                    notif.Subject = "New Request";
                    notif.Body = "A new request was created for " + model.Name + "!";
                    notif.TimeSent = DateTime.Now;
                    notif.RegistrationId = model.RegistrationId;

                    new NotificationService(_context).SendNotification(notif);


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
        /// Returns all registrations as RegisterModel entities
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult ViewAll()
        {
            var registrations = _context.RegisterModels
                .Include(r => r.DaySchedules)
                .ThenInclude(d => d.Rides)
                .ThenInclude(r => r.Route)
                .Where(r => !r.IsArchived)
                .ToList();

            return View("RegistrationTable", registrations);
        }

        /// <summary>
        /// Views the passenger lists
        /// </summary>
        /// <param name="routeId">The ID for the route that you are viewing</param>
        /// <returns></returns>
        public IActionResult ViewPassengerList(int routeId)
        {
            // Get the route including Pickup and Dropoff locations
            Routes route = _context.Routes
                .Include(r => r.PickUpLocation)    // assuming navigation property
                .Include(r => r.DropOffLocation)   // assuming navigation property
                .FirstOrDefault(r => r.RouteID == routeId);

            if (route != null)
            {
                // Build route info string
                string pickupName = route.PickUpLocation?.Name ?? "Unknown Pickup";
                string dropoffName = route.DropOffLocation?.Name ?? "Unknown Dropoff";
                string dayOfWeek = route.DayOfWeek.ToString();

                TempData["RouteInfo"] = $"Requests for Route: {dayOfWeek}, From: {pickupName} To {dropoffName}";
            }

            // Get registrations for this route
            var registrations = _context.RegisterModels
                .Include(r => r.DaySchedules)
                    .ThenInclude(ds => ds.Rides)
                .Where(r => r.DaySchedules
                    .Any(ds => ds.Rides
                        .Any(ride => ride.RouteId == routeId)))
                .ToList();

            return View("PassengerList", registrations);
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

            return DateTime.Today.Add(time.Value).ToString("h:mm tt");
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

            // Clear existing structure
            existing.DaySchedules.Clear();

            foreach (var modelDay in model.DaySchedules)
            {
                var newDay = new RequestDay
                {
                    WeekDay = modelDay.WeekDay,
                    Rides = new List<Ride>()
                };

                foreach (var modelRide in modelDay.Rides)
                {
                    var newRide = new Ride
                    {
                        PickUpLocationID = modelRide.PickUpLocationID,
                        DropOffLocationID = modelRide.DropOffLocationID,
                        DropOffTime = modelRide.DropOffTime,
                        RouteId = modelRide.RouteId
                    };

                    newDay.Rides.Add(newRide);
                }

                existing.DaySchedules.Add(newDay);
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
                    Value = time.ToString("HH:mm"),
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

            return RedirectToAction("ViewAll");
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

