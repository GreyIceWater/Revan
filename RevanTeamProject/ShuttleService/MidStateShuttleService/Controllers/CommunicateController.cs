using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
using System.Net;
using System.Net.Mail;

namespace MidStateShuttleService.Controllers
{
    public class CommunicateController : Controller
    {
        private readonly EmailServices _emailServices;

        private readonly ILogger<CommunicateController> _logger;

        private readonly ApplicationDbContext _context;

        // Inject ApplicationDbContext into the controller constructor
        public CommunicateController(ApplicationDbContext context, ILogger<CommunicateController> logger, EmailServices emailServices)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            _logger = logger; // Assign the injected ILogger to the _logger field
            _emailServices = emailServices;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index(int? routeId)
        {
            var model = new CommuncateModel();
            model.LocationNames = GetLocationNames();

            if (routeId.HasValue)
            {
                var route = _context.Routes
                    .Include(r => r.PickUpLocation)
                    .Include(r => r.DropOffLocation)
                    .FirstOrDefault(r => r.RouteID == routeId.Value);

                if (route != null)
                {
                    ViewData["RouteId"] = route.RouteID;
                    ViewData["RouteInfo"] = $"{route.PickUpLocation.Name} → {route.DropOffLocation.Name}";
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Index(CommuncateModel c, int? routeId)
        {
            c.LocationNames = GetLocationNames();
            if (ModelState.IsValid)
            {
                try
                {
                    CommunicationServices cs = new CommunicationServices(_context);
                    c.IsActive = true;
                    cs.AddEntity(c);
                    RegisterServices rs = new RegisterServices(_context);
                    var registeredStudents = rs.GetEmailsByRoute(routeId ?? 0);
                    foreach (var student in registeredStudents)
                    {
                        _emailServices.SendEmail(student.Email, "Mid State Shuttle Service Update", c.message);
                    }
                    TempData["CommunicationSuccess"] = true;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Sending Message");
                    return View("Error");
                }
            }
            return View(c);
        }

        [AllowAnonymous]
        public IActionResult MessageSent()
        {
            return View();
        }

        /// <summary>
        /// Displays the view for the student's communication form
        /// </summary>
        /// <returns> The Student Communicate View </returns>
        [AllowAnonymous]
        public IActionResult StudentCommunicate()
        {
            return View();
        }

        // When the form submits, this method will play out.
        [AllowAnonymous]
        [HttpPost]
        public IActionResult StudentCommunicate(Message c)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    MessageServices ms = new MessageServices(_context);
                    c.IsActive = true;
                    ms.AddEntity(c);

                    // Increment the message count in the session
                    int messageCount = HttpContext.Session.GetInt32("MessageCount") ?? 0;
                    messageCount++;

                    HttpContext.Session.SetInt32("MessageCount", messageCount);
                    // Optionally, save the last message or a summary
                    HttpContext.Session.SetString("LastMessage", "You have a new message!");

                    HttpContext.Session.SetString("CommunicationSuccess", "true");

                    TempData["CommunicationSuccess"] = true;

                    Notification notif = new Notification();
                    notif.Subject = "Shuttle Service Review!";
                    notif.Body = c.name + " Sent a message!";
                    notif.TimeSent = DateTime.Now;
                    notif.MessageId = c.id;

                    new NotificationService(_context).SendNotification(notif);

                    return RedirectToAction("StudentCommunicate");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Sending Message");

                    return View("Error");
                }
            }

            return View(c);
        }

        //The method which will get the location names from the database
        private IEnumerable<SelectListItem> GetLocationNames()
        {
            LocationServices ls = new LocationServices(_context);
            var locations = ls.GetLocationNames();

            return locations;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ViewAll(bool viewArchived = false)
        {
            var messages = _context.Messages.Where(m => m.IsActive == !viewArchived);

            ViewData["Archives"] = viewArchived;

            return View("MessagesTable", messages);
        }

        public IActionResult MessageRespond(int id)
        {
            var message = _context.Messages.FirstOrDefault(m => m.id == id);
            if (message == null)
            {
                return NotFound();
            }
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MessageRespond(int id, string responseMessage)
        {
            var message = _context.Messages.FirstOrDefault(m => m.id == id);
            if (message == null)
            {
                return NotFound();
            }

            try
            {
                string subject = "Message reply from Mid-State Shuttle Services";

                _emailServices.SendEmail(message.Email, subject, responseMessage);

                TempData["Success"] = "Response sent successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to send response: {ex.Message}";
                return View(message);
            }
        }

        // GET: DriverController/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                var message = _context.Messages.Find(id);

                if (message != null)
                {
                    message.IsActive = !message.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();
                }
                else
                {
                    // Handle the case where the driver with the specified id is not found
                    ModelState.AddModelError("", "Message not found.");
                    return View();
                }

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the message.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the driver, please try again.");

                // Return the view with an error message
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            var message = _context.Messages.Find(id);

            if (message == null)
                return NotFound();

            message.IsActive = true;
            _context.SaveChanges();

            return RedirectToAction("ViewAll", new { viewArchived = true });
        }
    }
}
