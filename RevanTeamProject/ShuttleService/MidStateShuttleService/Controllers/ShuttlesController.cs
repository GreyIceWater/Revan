using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;

namespace MidStateShuttleService.Controllers
{
    public class ShuttlesController : Controller
    {
        private readonly ILogger<ShuttlesController> _logger;
        private readonly ApplicationDbContext _context;

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
        [HttpGet]
        public ActionResult Create()
        {
            LoadDrivers();
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
                LoadDrivers();
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

                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the bus.");

                ModelState.AddModelError("", "An unexpected error occurred while creating the shuttle.");
                LoadDrivers();

                return View(bus);
            }
        }

        // GET: ShuttlesController/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            try
            {
                var bus = _context.Buses.Find(id);

                if (bus == null)
                {
                    return NotFound();
                }

                LoadDrivers();
                return View(bus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading shuttle {BusId} for edit.", id);
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // POST: ShuttlesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Bus bus)
        {
            if (bus == null || id != bus.BusId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                LoadDrivers();
                return View(bus);
            }

            try
            {
                var existingBus = _context.Buses.Find(id);

                if (existingBus == null)
                {
                    return NotFound();
                }

                // Update only editable properties
                existingBus.BusNo = bus.BusNo;
                existingBus.PassengerCapacity = bus.PassengerCapacity;
                existingBus.DriverId = bus.DriverId;

                // Preserve existing IsActive value
                _context.SaveChanges();

                TempData["SuccessMessage"] = "The bus has been successfully updated!";
                HttpContext.Session.SetString("ShuttleSuccess", "true");
                TempData["ShuttleSuccess"] = true;

                return RedirectToAction(nameof(Edit), new { id = existingBus.BusId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating shuttle {BusId}.", id);

                ModelState.AddModelError("", "An unexpected error occurred while updating the shuttle.");
                LoadDrivers();

                return View(bus);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Driver")]
        public ActionResult ViewAll(bool viewArchived = false)
        {
            var shuttles = _context.Buses
            .Where(b => b.IsActive == !viewArchived)
            .ToList();

            ViewData["Archives"] = viewArchived;

            return View("ShuttleTable", shuttles);
        }

        // GET: ShuttlesController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            try
            {
                var shuttle = _context.Buses.Find(id);

                if (shuttle == null)
                {
                    TempData["ErrorMessage"] = "Shuttle not found.";
                    return RedirectToAction("Index", "Dashboard");
                }

                bool isCurrentlyActive = shuttle.IsActive ?? false;
                shuttle.IsActive = !isCurrentlyActive;

                _context.Buses.Update(shuttle);
                _context.SaveChanges();

                TempData["SuccessMessage"] = shuttle.IsActive == true
                    ? "The shuttle has been restored successfully!"
                    : "The shuttle has been removed successfully!";

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while toggling IsActive of the shuttle {BusId}.", id);
                TempData["ErrorMessage"] = "An unexpected error occurred while updating the shuttle.";
                return RedirectToAction("Index", "Dashboard");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while posting delete for shuttle {BusId}.", id);
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            var shuttle = _context.Buses.Find(id);

            if (shuttle == null)
                return NotFound();

            shuttle.IsActive = true;
            _context.SaveChanges();

            return RedirectToAction("ViewAll", new { viewArchived = true });
        }

        private void LoadDrivers()
        {
            DriverServices ds = new DriverServices(_context);

            ViewBag.Drivers = ds.GetAllEntities()
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.DriverId.ToString()
                });
        }
    }
}