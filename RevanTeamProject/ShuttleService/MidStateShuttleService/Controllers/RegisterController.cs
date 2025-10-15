using Microsoft.AspNetCore.Mvc;
using MidStateShuttleService.Models;
using System.Diagnostics;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace MidStateShuttleService.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<RegisterController> _logger;

        // Inject ApplicationDbContext into the controller constructor
        public RegisterController(ApplicationDbContext context)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return LoadRegisterModel();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return LoadRegisterModel();
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
        [AllowAnonymous]
        public ActionResult Register(RegisterModel model)
        {
            LocationServices ls = new LocationServices(_context);
            RegisterServices rs = new RegisterServices(_context);
            EmailServices es = new EmailServices();

            // Repopulate LocationNames for the model in case of return to View due to invalid model state or any error.
            model.LocationNames = ls.GetLocationNames();

            if (ModelState.IsValid)
            {
                model.IsActive = true; // Set IsActive to true
                model.DeviceIpAddress = model.DeviceIpAddress ?? "Unknown"; // Default to "Unknown" if IP is null
                model.InsertDateTime = DateTime.Now;
                model.RequestStatus = RequestStatus.Pending;

                if (!model.PickUpLocationID.HasValue)
                    ModelState.AddModelError(nameof(model.PickUpLocationID), "Pick-up location must not be empty.");

                if (!model.DropOffLocationID.HasValue)
                    ModelState.AddModelError(nameof(model.DropOffLocationID), "Drop-off location must not be empty.");

                if (rs.AddEntity(model))
                {
                    // Increment the registration count in the session
                    int registrationCount = HttpContext.Session.GetInt32("RegistrationCount") ?? 0;
                    registrationCount++;

                    HttpContext.Session.SetString("RegistrationSuccess", "true"); // Using session to set registration success.
                    HttpContext.Session.SetInt32("RegistrationCount", registrationCount);

                    TempData["RegistrationSuccess"] = true;

                    string emailBody = GenerateRegistrationEmailBody(model);
                    es.SendEmail(model.Email, "MSTC Shuttle Service Request", emailBody, isHtml: true);

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "There was an error saving the request, please try again.");
                }
            }

            ViewBag.Terms = GetSchoolTermSelectList();

            return View("Index", model);
        }


        [AllowAnonymous]
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
            foreach( var r in routesList)
            {                
                if (r.AdditionalDetails != null)
                    formattedRoutesList.Add(new {
                        r.RouteID,
                        Detail = $"Leave {ls.getLocationNameById(r.PickUpLocationID)} at {r.ToStringPickUpTime()} ({r.AdditionalDetails}), Arrive at {ls.getLocationNameById(r.DropOffLocationID)} at {r.ToStringDropOffTime()}" });
                else
                    formattedRoutesList.Add(new {
                        r.RouteID,
                        Detail = $"Leave {ls.getLocationNameById(r.PickUpLocationID)} at {r.ToStringPickUpTime()}, Arrive at {ls.getLocationNameById(r.DropOffLocationID)} at {r.ToStringDropOffTime()}"
                    });
            }

            return Json(formattedRoutesList);
        }

        // GET: RegisterController/Edit/5
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
            ViewBag.RequestStatuses = GetRequestStatusSelectList();

            // Return the location names for each route
            foreach (Routes route in ViewBag.RouteList)
            {
                route.PickUpLocation = ls.GetEntityById(route.PickUpLocationID);
                route.DropOffLocation = ls.GetEntityById(route.DropOffLocationID);
            }

            return View(student);
        }


        // POST: RegisterController/Edit/5
        // POST: RegisterController/Edit/5
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
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context); // Log SQL exception
                _logger.LogError(ex, "An error occurred while updating student.");
                ModelState.AddModelError("", "An unexpected error occurred, please try again.");

                ViewBag.Terms = GetSchoolTermSelectList();

                return View(student); // Return the view with an error message
            }
        }

        // GET: RegisterController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var student = _context.RegisterModels.Find(id);

                if (student != null)
                {
                    student.IsActive = !student.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();
                }
                else
                {
                    // Handle the case where the student with the specified id is not found
                    ModelState.AddModelError("", "Student not found.");
                    return View();
                }

                return RedirectToAction("Index", "Dashboard"); // Redirect after toggling IsActive
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the student.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the student, please try again.");

                // Return the view with an error message
                return View();
            }

        }

        // POST: RegisterController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
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
                // Check if the pick-up and drop-off locations are valid
                if (model.PickUpLocationID == null || model.DropOffLocationID == null)
                {
                    return "Invalid pick-up or drop-off location";
                }
                else
                {
                    ActionResult initalActionResult;
                    ActionResult returnActionResult;

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

                    try
                    {
                        // Outbound (required)
                        var initialRoute = FormatRoute(model.Route);

                        // Return (optional)
                        var returnRoute = model.ReturnRoute != null
                            ? FormatRoute(model.ReturnRoute)
                            : "One-way trip (no return selected)";

                        return BuildEmailConfirmationBody(model, initialRoute, returnRoute);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating registration email body");
                        return "An error occurred while generating registration email body.";
                    }


                }
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while generating request email body.");
                return "An error occurred while generating request email body.";
            }            
        }

        private List<SelectListItem> GetRequestStatusSelectList()
        {
            return Enum.GetValues(typeof(RequestStatus))
                .Cast<RequestStatus>()
                .Select(status => new SelectListItem
                {
                    Text = GetEnumDisplayName(status),
                    Value = status.ToString()
                }).ToList();
        }

        private List<SelectListItem> GetSchoolTermSelectList()
        {
            return Enum.GetValues(typeof(SchoolTerm))
                .Cast<SchoolTerm>()
                .Select(term => new SelectListItem
                {
                    Text = GetEnumDisplayName(term),
                    Value = term.ToString()
                }).ToList();
        }

        private string GetEnumDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName() ?? enumValue.ToString();
        }

        private string FormatRoute(Routes? route)
        {
            if (route == null)
                return "Unknown";

            return $"Leave {route.PickUpLocation?.Name ?? "Unknown"} at {route.PickUpTime?.ToString("h:mm tt") ?? "N/A"}, " +
                   $"Arrive at {route.DropOffLocation?.Name ?? "Unknown"} at {route.DropOffTime?.ToString("h:mm tt") ?? "N/A"}";
        }

        /// <summary>
        /// Build the route for special requests in the email.
        /// </summary>
        /// <param name="model">Model of the request.</param>
        /// <returns>The email confirmation body for a special request.</returns>
        private string SpecialRequestRoute(RegisterModel model)
        {
            var initialRoute = "";
            return BuildEmailConfirmationBody(model, initialRoute);
                //model.Term.Value.ToString(),
                //model.StudentId,
                //model.FirstName,
                //model.LastName,
                //model.IsAdult,
                //model.Email,
                //model.PhoneNumber,
                //initialRoute,
                //model.TripType,
                //model.SelectedDaysOfWeek,
                //model.FirstDayExpectingToRide);
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
        private string ParseRouteResult(ActionResult actionResult)
        {
            string route;

            if (actionResult is JsonResult jsonResult)
            {
                string jsonString = JsonSerializer.Serialize(jsonResult.Value);

                // Parse the JSON string
                using JsonDocument doc = JsonDocument.Parse(jsonString);

                // Assuming the first route in the list is required
                route = doc.RootElement[0].GetProperty("Detail").GetString();

                return route;
            }
            else
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Builds the email confirmation body for a registration request.
        /// </summary>
        /// <param name="model">RegisterModel containing the info for the email.</param>
        /// <param name="initialRoute">Initial route the student is taking.</param>
        /// <param name="returnRoute">Return route, if applicable, to get the student back.</param>
        /// <returns></returns>
        private string BuildEmailConfirmationBody(RegisterModel model, string initialRoute, string? returnRoute = null)
        {
            string isAdultText = model.IsAdult ? "Yes" : "No";

            // Build the return route HTML only if one exists
            string returnRouteHtml = !string.IsNullOrWhiteSpace(returnRoute) && returnRoute != "None Selected"
                ? $"<p><strong>Return Route:</strong> {returnRoute}</p>"
                : string.Empty;

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .email-container {{ max-width: 600px; margin: 0; padding: 20px; }}
                        .content {{ margin-top: 20px; }}
                        .content span {{ color: black; text-decoration: none; }}
                        .footer {{ margin-top: 30px; text-align: center; font-size: 12px; color: gray; padding-top: 20px; width: 100%; }}
                        .footer p {{ display: block; margin: 5px 0; }}
                        .header h2 {{ color: black; }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <h2>MSTC Shuttle Service Request Confirmation</h2>
                        </div>
                        <div class='content'>
                            <p><strong>School Term:</strong> {model.Term?.ToString() ?? "N/A"}</p>
                            <p><strong>Student ID:</strong> {model.StudentId}</p>
                            <p><strong>First Name:</strong> {model.FirstName}</p>
                            <p><strong>Last Name:</strong> {model.LastName}</p>
                            <p><strong>I am 18 years of age or older:</strong> {isAdultText}</p>
                            <p><strong>Email:</strong> {model.Email}</p>
                            <p><strong>Phone Number:</strong> {model.PhoneNumber}</p>
                            <p><strong>Initial Route:</strong> {initialRoute}</p>
                            {returnRouteHtml}
                            <p><strong>Trip Type:</strong> {model.TripType}</p>
                            <p><strong>Days of the Week Needed:</strong> {(model.SelectedDaysOfWeek != null && model.SelectedDaysOfWeek.Any() ? string.Join(", ", model.SelectedDaysOfWeek) : "None")}</p>
                            <p><strong>First Day Expecting to Ride:</strong> {model.FirstDayExpectingToRide?.ToString("MM-dd-yyyy")}</p>
                        </div>
                        <div class='footer'>
                            <p>Thank you {model.FirstName} for submitting your shuttle request. Your ride is <strong>NOT</strong> confirmed yet. The Mid-State shuttle team will review your request, and a response will be shared via email.
                            If you have any questions, please call or text: <strong>715-581-9284</strong></p>
                            <div>&nbsp;</div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        /// <summary>
        /// Centralized method to load a RegisterModel with locations and routes
        /// </summary>
        private IActionResult LoadRegisterModel()
        {
            var model = new RegisterModel();
            var ls = new LocationServices(_context);
            var rs = new RouteServices(_context);

            model.LocationNames = ls.GetLocationNames();

            var activeRoutes = rs.GetAllEntities().Where(r => r.IsActive).ToList();

            foreach (var route in activeRoutes)
            {
                route.PickUpLocation = ls.GetEntityById(route.PickUpLocationID);
                route.DropOffLocation = ls.GetEntityById(route.DropOffLocationID);
            }

            if (model.ReturnRoute != null)
            {
                model.ReturnPickUpLocationId = model.ReturnRoute.PickUpLocationID;
                model.ReturnDropOffLocationId = model.ReturnRoute.DropOffLocationID;
            }

            model.Routes = activeRoutes;

            ViewBag.Terms = GetSchoolTermSelectList();
            ViewBag.RequestStatuses = GetRequestStatusSelectList();

            return View("Index", model);
        }
    }
}

