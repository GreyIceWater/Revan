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

            _logger.LogInformation("RegisterController initialized.");
        }

        //overload method for default
        private List<SelectListItem> GetSchoolTermSelectList()
        {
            _logger.LogInformation("GetSchoolTermSelectList() called.");
            return GetSchoolTermSelectList(false);
        }

        /// <summary>
        /// Returns the list of Terms
        /// </summary>
        /// <param name="getSummer"></param>
        /// <returns></returns>
        private List<SelectListItem> GetSchoolTermSelectList(bool isSpecial)
        {
            _logger.LogInformation("GetSchoolTermSelectList(bool) called. isSpecial: {IsSpecial}", isSpecial);

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
            _logger.LogInformation("GetEnumDisplayName called for enum value: {EnumValue}", enumValue);

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
            _logger.LogInformation("Index action called.");

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

            _logger.LogInformation("Index action returning view.");
            return View(model);
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy action called.");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("Error action called. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Completed the backend logic for a registration form submission
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            _logger.LogInformation("Register action called for Name: {Name}, Email: {Email}, IsCustom: {IsCustom}", model?.Name, model?.Email, model?.isCustom);

            LocationServices ls = new LocationServices(_context);
            RegisterServices rs = new RegisterServices(_context);

            model.TimeOptions = GetTimeSelectList();

            model.InsertDateTime = DateTime.UtcNow;

            // Repopulate LocationNames in case we return to the view
            model.LocationNames = ls.GetLocationNames();

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid for registration submission.");

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
                        _logger.LogWarning("Duplicate request days detected: {DuplicateDays}", string.Join(", ", duplicateDays));
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
                        _logger.LogWarning("Registration failed validation because no rides were provided.");
                        TempData["Error"] = "At least one ride must be added to submit a registration.";
                        ViewBag.Terms = GetSchoolTermSelectList();
                        return View("Index", model);
                    }

                    // Ensure no request day has zero rides
                    bool emptyDayExists = model.DaySchedules != null &&
                                          model.DaySchedules.Any(d => d.Rides == null || !d.Rides.Any());

                    if (emptyDayExists)
                    {
                        _logger.LogWarning("Registration failed validation because at least one request day had zero rides.");
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
                    _logger.LogWarning("Registration failed validation because at least one ride was missing both route and drop-off time.");
                    TempData["Error"] = "Each ride must have either a route selected or a drop-off time.";
                    ViewBag.Terms = GetSchoolTermSelectList();
                    return View("Index", model);
                }

                // -------- SAVE REGISTRATION --------

                if (rs.AddEntity(model))
                {
                    _logger.LogInformation("Registration saved successfully. RegistrationId: {RegistrationId}", model.RegistrationId);

                    int registrationCount = HttpContext.Session.GetInt32("RegistrationCount") ?? 0;
                    registrationCount++;

                    HttpContext.Session.SetInt32("RegistrationCount", registrationCount);

                    string emailBody = "";

                    if (model.isCustom)
                    {
                        _logger.LogInformation("Building special registration email for RegistrationId: {RegistrationId}", model.RegistrationId);
                        emailBody = BuildEmailForSpecialRegisterSubmit(model.RegistrationId);
                    }
                    else
                    {
                        _logger.LogInformation("Building standard registration email for RegistrationId: {RegistrationId}", model.RegistrationId);
                        emailBody = BuildEmailForRegisterSubmit(model.RegistrationId);
                    }

                    _emailServices.SendEmailToAdmin(
                        "MSTC Shuttle Service Request Confirmation",
                        emailBody,
                        isHtml: true
                    );

                    _logger.LogInformation("Admin email sent for RegistrationId: {RegistrationId}", model.RegistrationId);

                    //Send notification for the admin page

                    Notification notif = new Notification();
                    notif.Subject = "New Request";
                    notif.Body = "A new request was created for " + model.Name + "!";
                    notif.TimeSent = DateTime.Now;
                    notif.RegistrationId = model.RegistrationId;

                    new NotificationService(_context).SendNotification(notif);

                    _logger.LogInformation("Notification sent for RegistrationId: {RegistrationId}", model.RegistrationId);

                    TempData["Success"] = "Registration created successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogError("Registration save failed for Name: {Name}, Email: {Email}", model.Name, model.Email);
                    TempData["Error"] = "Something went wrong.";
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid for registration submission.");
            }

            ViewBag.Terms = GetSchoolTermSelectList();
            _logger.LogInformation("Register action returning Index view due to validation or save failure.");
            return View("Index", model);
        }

        /// <summary>
        /// Returns all registrations as RegisterModel entities
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult ViewAll(bool viewArchived = false)
        {
            _logger.LogInformation("ViewAll action called. viewArchived: {ViewArchived}", viewArchived);

            var registrations = _context.RegisterModels
                .Include(r => r.DaySchedules)
                .ThenInclude(d => d.Rides)
                .ThenInclude(r => r.Route)
                .Where(r => r.IsArchived == viewArchived)
                .ToList();

            ViewData["Archives"] = viewArchived;

            _logger.LogInformation("ViewAll action returning {Count} registrations.", registrations.Count);
            return View("RegistrationTable", registrations);
        }

        /// <summary>
        /// Views the passenger lists
        /// </summary>
        /// <param name="routeId">The ID for the route that you are viewing</param>
        /// <returns></returns>
        public IActionResult ViewPassengerList(int routeId)
        {
            _logger.LogInformation("ViewPassengerList action called for RouteId: {RouteId}", routeId);

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
                _logger.LogInformation("Route info built for RouteId: {RouteId}", routeId);
            }
            else
            {
                _logger.LogWarning("No route found for RouteId: {RouteId}", routeId);
            }

            // Get registrations for this route
            var registrations = _context.RegisterModels
                .Include(r => r.DaySchedules)
                    .ThenInclude(ds => ds.Rides)
                .Where(r => r.DaySchedules
                    .Any(ds => ds.Rides
                        .Any(ride => ride.RouteId == routeId)))
                .ToList();

            _logger.LogInformation("ViewPassengerList returning {Count} registrations for RouteId: {RouteId}", registrations.Count, routeId);
            return View("PassengerList", registrations);
        }

        // Displays the full breakdown of a single registration
        [Authorize(Roles = "Admin")]
        public IActionResult Details(int registrationId)
        {
            _logger.LogInformation("Details action called for RegistrationId: {RegistrationId}", registrationId);

            var registration = _context.RegisterModels
            .Include(r => r.DaySchedules)
                .ThenInclude(d => d.Rides)
                    .ThenInclude(r => r.Route)
            .FirstOrDefault(r => r.RegistrationId == registrationId);

            if (registration == null)
            {
                _logger.LogWarning("Details action could not find RegistrationId: {RegistrationId}", registrationId);
                return NotFound();
            }

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

            _logger.LogInformation("Details action returning view for RegistrationId: {RegistrationId}", registrationId);
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
            _logger.LogInformation("SpecialDetails action called for RegistrationId: {RegistrationId}", registrationId);

            var model = _context.RegisterModels
                .FirstOrDefault(r => r.RegistrationId == registrationId);

            if (model == null)
            {
                _logger.LogWarning("SpecialDetails action could not find RegistrationId: {RegistrationId}", registrationId);
                return NotFound();
            }

            ViewBag.Terms = GetSchoolTermSelectList(true);

            _logger.LogInformation("SpecialDetails action returning view for RegistrationId: {RegistrationId}", registrationId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult EditSave(RegisterModel model)
        {
            _logger.LogInformation("EditSave action called for RegistrationId: {RegistrationId}", model?.RegistrationId);

            LocationServices ls = new LocationServices(_context);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("EditSave ModelState invalid for RegistrationId: {RegistrationId}", model?.RegistrationId);

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
            {
                _logger.LogWarning("EditSave could not find RegistrationId: {RegistrationId}", model.RegistrationId);
                return NotFound();
            }

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

            _logger.LogInformation("EditSave completed successfully for RegistrationId: {RegistrationId}", existing.RegistrationId);
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
            _logger.LogInformation("SpecialEditSave action called for RegistrationId: {RegistrationId}", model?.RegistrationId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("SpecialEditSave ModelState invalid for RegistrationId: {RegistrationId}", model?.RegistrationId);
                ViewBag.Terms = GetSchoolTermSelectList(true);
                return View("SpecialDetails", model);
            }

            var existing = _context.RegisterModels
                .FirstOrDefault(r => r.RegistrationId == model.RegistrationId);

            if (existing == null)
            {
                _logger.LogWarning("SpecialEditSave could not find RegistrationId: {RegistrationId}", model.RegistrationId);
                return NotFound();
            }

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

            _logger.LogInformation("SpecialEditSave completed successfully for RegistrationId: {RegistrationId}", existing.RegistrationId);
            return RedirectToAction("SpecialDetails",
                new { registrationId = existing.RegistrationId });
        }

        private List<SelectListItem> GetTimeSelectList()
        {
            _logger.LogInformation("GetTimeSelectList called.");

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

            _logger.LogInformation("GetTimeSelectList returning {Count} time options.", times.Count);
            return times;
        }

        [HttpGet]
        public ActionResult SpecialRequest()
        {
            _logger.LogInformation("SpecialRequest action called.");

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

            _logger.LogInformation("SpecialRequest action returning view.");
            return View("SpecialRequest", model);
        }

        [Authorize]
        public ActionResult RegisterConfirmation(RegisterModel model)
        {
            _logger.LogInformation("RegisterConfirmation action called for RegistrationId: {RegistrationId}", model?.RegistrationId);

            if (ModelState.IsValid)
            {
                _logger.LogInformation("RegisterConfirmation ModelState valid.");
                return View(model);
            }

            _logger.LogWarning("RegisterConfirmation ModelState invalid.");
            return View("Index", model);
        }


        //retrieves route options based on selected pick-up and drop-off locations from a database and returns them as JSON.
        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetRoutes(int pickUpLocationId, int dropOffLocationId)
        {
            _logger.LogInformation("GetRoutes called. PickUpLocationId: {PickUpLocationId}, DropOffLocationId: {DropOffLocationId}", pickUpLocationId, dropOffLocationId);

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

            _logger.LogInformation("GetRoutes returning {Count} routes.", formattedRoutesList.Count);
            return Json(formattedRoutesList);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            _logger.LogInformation("Unarchive action called for RegistrationId: {RegistrationId}", id);

            var registration = _context.RegisterModels.Find(id);

            if (registration == null)
            {
                _logger.LogWarning("Unarchive could not find RegistrationId: {RegistrationId}", id);
                return NotFound();
            }

            registration.IsArchived = false;
            _context.SaveChanges();

            _logger.LogInformation("Unarchive completed for RegistrationId: {RegistrationId}", id);
            return RedirectToAction("ViewAll", new { viewArchived = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ArchiveRegistration(int id)
        {
            _logger.LogInformation("ArchiveRegistration action called for RegistrationId: {RegistrationId}", id);

            var registration = _context.RegisterModels.FirstOrDefault(r => r.RegistrationId == id);
            if (registration == null)
            {
                _logger.LogWarning("ArchiveRegistration could not find RegistrationId: {RegistrationId}", id);
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning("ArchiveRegistration forbidden for RegistrationId: {RegistrationId}. User is not Admin.", id);
                return Forbid();
            }

            registration.IsArchived = true;
            _context.SaveChanges();

            _logger.LogInformation("ArchiveRegistration completed for RegistrationId: {RegistrationId}", id);
            return RedirectToAction("ViewAll");
        }
        private JsonResult GetRouteInfo(int pickUpLocationId, int dropOffLocationId)
        {
            _logger.LogInformation("GetRouteInfo called. PickUpLocationId: {PickUpLocationId}, DropOffLocationId: {DropOffLocationId}", pickUpLocationId, dropOffLocationId);

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

            _logger.LogInformation("GetRouteInfo returning {Count} routes.", formattedRoutesList.Count);
            return Json(formattedRoutesList);
        }

        /// <summary>
        /// Convert ride times to a string format for display in the email.
        /// </summary>
        /// <param name="time">Time to return as a string.</param>
        /// <returns>A string formatted time.</returns>
        private string ToStringRideTimes(TimeOnly? time)
        {
            _logger.LogInformation("ToStringRideTimes called. HasValue: {HasValue}", time.HasValue);

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
            _logger.LogInformation("ParseInitialResult called.");

            if (actionResult is JsonResult jsonResult)
            {
                string jsonString = JsonSerializer.Serialize(jsonResult.Value);

                // Parse the JSON string
                using JsonDocument doc = JsonDocument.Parse(jsonString);

                // Assuming the first route in the list is required
                initialRoute = doc.RootElement[0].GetProperty("Detail").GetString();
            }

            _logger.LogInformation("ParseInitialResult completed.");
            return initialRoute;
        }

        /// <summary>
        /// Builds the email confirmation body for a registration request.
        /// </summary>
        private string BuildEmailForRegisterSubmit(int id)
        {
            _logger.LogInformation("BuildEmailForRegisterSubmit called for RegistrationId: {RegistrationId}", id);

            var registration = _context.RegisterModels
                .Include(r => r.DaySchedules)
                    .ThenInclude(d => d.Rides)
                        .ThenInclude(r => r.PickUpLocation)
                .Include(r => r.DaySchedules)
                    .ThenInclude(d => d.Rides)
                        .ThenInclude(r => r.DropOffLocation)
                .Include(r => r.DaySchedules)
                    .ThenInclude(d => d.Rides)
                        .ThenInclude(r => r.Route)
                            .ThenInclude(r => r.PickUpLocation)
                .Include(r => r.DaySchedules)
                    .ThenInclude(d => d.Rides)
                        .ThenInclude(r => r.Route)
                            .ThenInclude(r => r.DropOffLocation)
                .FirstOrDefault(r => r.RegistrationId == id);

            if (registration == null)
            {
                _logger.LogWarning("BuildEmailForRegisterSubmit could not find RegistrationId: {RegistrationId}", id);
                return "<p>Registration not found.</p>";
            }

            string isAdultText = registration.IsAdult ? "Adult Rider" : "Minor Rider";
            string sId = string.IsNullOrEmpty(registration.StudentId) ? "N/A" : registration.StudentId;

            string scheduleSections = "";

            if (registration.DaySchedules != null)
            {
                foreach (var day in registration.DaySchedules)
                {
                    string rideRows = "";

                    if (day.Rides != null && day.Rides.Any())
                    {
                        foreach (var ride in day.Rides)
                        {
                            string dropOffTime = ride.DropOffTime.HasValue
                                ? ride.DropOffTime.Value.ToString("hh:mm tt")
                                : "N/A";

                            if (ride.Route != null)
                            {
                                string routeLeaveTime = ride.Route.PickUpTime.HasValue
                                    ? DateTime.Today.Add(ride.Route.PickUpTime.Value).ToString("hh:mm tt")
                                    : "N/A";
                                string routeArriveTime = ride.Route.DropOffTime.HasValue
                                    ? DateTime.Today.Add(ride.Route.DropOffTime.Value).ToString("hh:mm tt")
                                    : "N/A";

                                rideRows += $@"
                                <tr>
                                    <td style='padding: 10px 16px;'>{ride.Route.PickUpLocation?.Name ?? "Unknown"}</td>
                                    <td style='padding: 10px 16px;'>{ride.Route.DropOffLocation?.Name ?? "Unknown"}</td>
                                    <td style='padding: 10px 16px;'>{routeLeaveTime} > {routeArriveTime}</td>
                                </tr>";
                            }
                            else
                            {
                                rideRows += $@"
                                <tr>
                                    <td style='padding: 10px 16px;'>{ride.PickUpLocation?.Name ?? "Unknown"}</td>
                                    <td style='padding: 10px 16px;'>{ride.DropOffLocation?.Name ?? "Unknown"}</td>
                                    <td style='padding: 10px 16px;'>{dropOffTime}</td>
                                </tr>";
                            }
                        }
                    }
                    else
                    {
                        rideRows = "<tr><td colspan='3' style='padding: 10px 16px; color:#888;'>No rides scheduled for this day.</td></tr>";
                    }

                    scheduleSections += $@"
                <h3 style='margin: 24px 0 8px; color: #8B0000;'>{day.WeekDay}</h3>
                <table width='100%' cellpadding='0' cellspacing='0' style='border-collapse: collapse; border: 1px solid #ddd; border-radius: 6px; overflow: hidden; margin-bottom: 16px;'>
                    <thead>
                        <tr style='background-color: #8B0000; color: white;'>
                            <th style='padding: 10px 16px; text-align: left;'>Pick-Up</th>
                            <th style='padding: 10px 16px; text-align: left;'>Drop-Off</th>
                            <th style='padding: 10px 16px; text-align: left;'>Arrival Time</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rideRows}
                    </tbody>
                </table>";
                }
            }

            _logger.LogInformation("BuildEmailForRegisterSubmit completed for RegistrationId: {RegistrationId}", id);

            return $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333; max-width: 680px; margin: 0 auto; padding: 24px;'>

    <div style='background-color: #8B0000; padding: 24px; border-radius: 6px 6px 0 0;'>
        <h1 style='margin: 0; color: white; font-size: 20px;'>MSTC Shuttle Service</h1>
        <p style='margin: 4px 0 0; color: #f5c0c0; font-size: 14px;'>Registration Request Confirmation</p>
    </div>

    <div style='background: #fff; border: 1px solid #ddd; border-top: none; padding: 24px; border-radius: 0 0 6px 6px;'>

        <h2 style='margin: 0 0 16px; font-size: 18px;'>{registration.Name}</h2>

        <table cellpadding='0' cellspacing='0' style='width: 100%; margin-bottom: 24px;'>
            <tr>
                <td style='padding: 6px 0; color: #888; width: 140px;'>Student ID</td>
                <td style='padding: 6px 0;'>{sId}</td>
            </tr>
            <tr>
                <td style='padding: 6px 0; color: #888;'>Email</td>
                <td style='padding: 6px 0;'>{registration.Email}</td>
            </tr>
            <tr>
                <td style='padding: 6px 0; color: #888;'>Phone</td>
                <td style='padding: 6px 0;'>{registration.Phone}</td>
            </tr>
            <tr>
                <td style='padding: 6px 0; color: #888;'>Rider Status</td>
                <td style='padding: 6px 0;'>
                    <span style='background-color: {(registration.IsAdult ? "#e6f4ea" : "#fff3e0")}; color: {(registration.IsAdult ? "#2e7d32" : "#e65100")}; padding: 2px 10px; border-radius: 12px; font-size: 13px;'>
                        {isAdultText}
                    </span>
                </td>
            </tr>
        </table>

        <hr style='border: none; border-top: 1px solid #eee; margin-bottom: 16px;'/>

        <h3 style='margin: 0 0 12px; font-size: 16px;'>Requested Schedule</h3>

        {scheduleSections}

        <hr style='border: none; border-top: 1px solid #eee; margin: 24px 0 16px;'/>

        <p style='margin: 0; font-size: 13px; color: #888;'>
            This request will be reviewed by the shuttle program and is not guaranteed.
        </p>

    </div>

</body>
</html>";
        }

        /// <summary>
        /// Builds the email confirmation body for a SPECIAL registration request.
        /// </summary>
        private string BuildEmailForSpecialRegisterSubmit(int id)
        {
            _logger.LogInformation("BuildEmailForSpecialRegisterSubmit called for RegistrationId: {RegistrationId}", id);

            var registration = _context.RegisterModels
                .FirstOrDefault(r => r.RegistrationId == id);

            if (registration == null)
            {
                _logger.LogWarning("BuildEmailForSpecialRegisterSubmit could not find RegistrationId: {RegistrationId}", id);
                return "<p>Registration not found.</p>";
            }

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

            _logger.LogInformation("BuildEmailForSpecialRegisterSubmit completed for RegistrationId: {RegistrationId}", id);
            return html;
        }
    }
}