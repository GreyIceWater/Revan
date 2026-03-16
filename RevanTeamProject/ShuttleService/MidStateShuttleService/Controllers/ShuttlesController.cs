using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using System.Data;

namespace MidStateShuttleService.Controllers
{
    public class ShuttlesController : Controller
    {
        private readonly ILogger<ShuttlesController> _logger;
        private readonly ApplicationDbContext _context;

        // Inject ApplicationDbContext and ILogger into the controller constructor
        public ShuttlesController(ApplicationDbContext context, ILogger<ShuttlesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ShuttlesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ShuttlesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ShuttlesController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            DriverServices ds = new DriverServices(_context);
            ViewBag.Drivers = ds.GetAllEntities().Select(x => new SelectListItem { Text = x.Name, Value = x.DriverId.ToString() });

            return View();
        }

        // POST: ShuttlesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Bus bus)
        {
            if (!ModelState.IsValid)
            {
                return View(bus);
            }

            try
            {
                BusServices bs = new BusServices(_context);
                bus.IsActive = true;
                bs.AddEntity(bus);

                TempData["SuccessMessage"] = "The bus has been successfully created!";
                HttpContext.Session.SetString("ShuttleSuccess", "true");
                TempData["ShuttleSuccess"] = true;

                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while creating the bus.");
                // You can return a specific view indicating failure or redirect to a generic error page
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: ShuttlesController/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            // Retrieve the bus from the database based on the id
            var bus = _context.Buses.Find(id);

            if (bus == null)
            {
                return NotFound(); // Return 404 if bus not found
            }

            // Load drivers for dropdown list
            DriverServices ds = new DriverServices(_context);
            ViewBag.Drivers = ds.GetAllEntities().Select(x => new SelectListItem { Text = x.Name, Value = x.DriverId.ToString() });

            return View(bus);
        }

        // POST: ShuttlesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Bus bus)
        {
            if (id != bus.BusId)
            {
                return BadRequest(); // Return bad request if IDs don't match
            }

            if (!ModelState.IsValid)
            {
                return View(bus); // Return view with errors if model is invalid
            }

            try
            {
                bus.IsActive = true;
                _context.Update(bus);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "The bus has been successfully updated!";
                HttpContext.Session.SetString("ShuttleSuccess", "true");
                TempData["ShuttleSuccess"] = true;
                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while updating the bus.");
                // Redirect to error page or handle the error appropriately
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Driver")]
        public ActionResult ViewAll()
        {
            var shuttles = new BusServices(_context).GetAllEntities();

            return View("ShuttleTable", shuttles);
        }

        // GET: ShuttlesController/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                var shuttle = _context.Buses.Find(id);

                if (shuttle == null)
                {
                    return NotFound(); // Return 404 if shuttle not found
                }

                shuttle.IsActive = !shuttle.IsActive; // Toggle IsActive value

                _context.SaveChanges();

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the shuttle.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the shuttle, please try again.");

                // Return the view with an error message or handle the error as required
                return View();
            }

        }

        // POST: ShuttlesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
    }
}
