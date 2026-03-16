using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using System.Data;

namespace MidStateShuttleService.Controllers
{
    public class DriverController : Controller
    {
        private readonly ILogger<DriverController> _logger;
        private readonly ApplicationDbContext _context;

        // Inject ApplicationDbContext into the controller constructor
        public DriverController(ApplicationDbContext context, ILogger<DriverController> logger)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            _logger = logger; // Assign the injected ILogger to the _logger field
        }

        // GET: DriverController
        public ActionResult Index()
        {
            return View();
        }

        // GET: DriverController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DriverController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: DriverController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Driver driver)
        {
            if (!ModelState.IsValid)
            {
                return View(driver);
            }

            try
            {
                DriverServices ds = new DriverServices(_context);
                driver.IsActive = true;
                ds.AddEntity(driver);

                TempData["SuccessMessage"] = "The driver has been successfully created!";
                HttpContext.Session.SetString("DriverSuccess", "true");
                TempData["DriverSuccess"] = true;

                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context); // Log SQL exception
                _logger.LogError(ex, "An error occurred while creating driver.");
                ModelState.AddModelError("", "An unexpected error occurred, please try again.");
                return View(driver);
            }

            
        }



        // GET: DriverController/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            // Retrieve the driver to be edited from the database
            var driver = _context.Drivers.Find(id);

            if (driver == null)
            {
                return NotFound(); // Or handle the case where the driver is not found
            }

            return View(driver);
        }

        // POST: DriverController/Edit/5
        // POST: DriverController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Driver driver)
        {
            if (id != driver.DriverId)
            {
                return BadRequest(); // Or handle the case where IDs do not match
            }

            if (!ModelState.IsValid)
            {
                return View(driver); // Return the view with validation errors
            }

            try
            {
                
                driver.IsActive = true; // Set IsActive to true
                // Update the driver in the database
                _context.Update(driver);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "The driver has been successfully updated!";
                HttpContext.Session.SetString("DriverSuccess", "true");
                TempData["DriverSuccess"] = true;
                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context); // Log SQL exception
                _logger.LogError(ex, "An error occurred while updating driver.");
                ModelState.AddModelError("", "An unexpected error occurred, please try again.");
                return View(driver); // Return the view with an error message
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ViewAll()
        {
            var drivers = new DriverServices(_context).GetAllEntities();

            return View("DriverTable", drivers);
        }

        // GET: DriverController/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                var driver = _context.Drivers.Find(id);

                if (driver != null)
                {
                    driver.IsActive = !driver.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();
                }
                else
                {
                    // Handle the case where the driver with the specified id is not found
                    ModelState.AddModelError("", "Driver not found.");
                    return View();
                }

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the driver.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the driver, please try again.");

                // Return the view with an error message
                return View();
            }          
        }


        // POST: DriverController/Delete/5
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
