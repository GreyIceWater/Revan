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

            _logger.LogInformation("ShuttlesController initialized.");
        }

        // GET: ShuttlesController
        public ActionResult Index()
        {
            _logger.LogInformation("Shuttles Index action called.");
            return View();
        }

        // GET: ShuttlesController/Details/5
        public ActionResult Details(int id)
        {
            _logger.LogInformation("Shuttles Details action called for BusId: {BusId}", id);
            return View();
        }

        // GET: ShuttlesController/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            _logger.LogInformation("Shuttles Create GET action called.");
            LoadDrivers();
            return View();
        }

        // POST: ShuttlesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Bus bus)
        {
            _logger.LogInformation("Shuttles Create POST action called.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Shuttles Create POST failed ModelState validation.");
                LoadDrivers();
                return View(bus);
            }

            try
            {
                BusServices bs = new BusServices(_context);

                bus.IsActive = true;
                bs.AddEntity(bus);

                _logger.LogInformation("Bus created successfully.");

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
            _logger.LogInformation("Shuttles Edit GET action called for BusId: {BusId}", id);

            try
            {
                var bus = _context.Buses.Find(id);

                if (bus == null)
                {
                    _logger.LogWarning("Shuttles Edit GET could not find BusId: {BusId}", id);
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
            _logger.LogInformation("Shuttles Edit POST action called for BusId: {BusId}", id);

            if (bus == null || id != bus.BusId)
            {
                _logger.LogWarning("Shuttles Edit POST received invalid bus data for BusId: {BusId}", id);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Shuttles Edit POST failed ModelState validation for BusId: {BusId}", id);
                LoadDrivers();
                return View(bus);
            }

            try
            {
                var existingBus = _context.Buses.Find(id);

                if (existingBus == null)
                {
                    _logger.LogWarning("Shuttles Edit POST could not find BusId: {BusId}", id);
                    return NotFound();
                }

                // Update only editable properties
                existingBus.BusNo = bus.BusNo;
                existingBus.PassengerCapacity = bus.PassengerCapacity;
                existingBus.DriverId = bus.DriverId;

                // Preserve existing IsActive value
                _context.SaveChanges();

                _logger.LogInformation("Shuttle updated successfully for BusId: {BusId}", id);

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
            _logger.LogInformation("Shuttles ViewAll action called. ViewArchived: {ViewArchived}", viewArchived);

            var shuttles = _context.Buses
            .Where(b => b.IsActive == !viewArchived)
            .ToList();

            ViewData["Archives"] = viewArchived;

            _logger.LogInformation("Shuttles ViewAll returning {ShuttleCount} shuttles.", shuttles.Count);
            return View("ShuttleTable", shuttles);
        }

        // GET: ShuttlesController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("Shuttles Delete GET action called for BusId: {BusId}", id);

            try
            {
                var shuttle = _context.Buses.Find(id);

                if (shuttle == null)
                {
                    _logger.LogWarning("Shuttles Delete GET could not find BusId: {BusId}", id);
                    TempData["ErrorMessage"] = "Shuttle not found.";
                    return RedirectToAction("Index", "Dashboard");
                }

                bool isCurrentlyActive = shuttle.IsActive ?? false;
                shuttle.IsActive = !isCurrentlyActive;

                _context.Buses.Update(shuttle);
                _context.SaveChanges();

                _logger.LogInformation("Shuttle IsActive toggled successfully for BusId: {BusId}", id);

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
            _logger.LogInformation("Shuttles Delete POST action called for BusId: {BusId}", id);

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
            _logger.LogInformation("Shuttles Unarchive action called for BusId: {BusId}", id);

            var shuttle = _context.Buses.Find(id);

            if (shuttle == null)
            {
                _logger.LogWarning("Shuttles Unarchive could not find BusId: {BusId}", id);
                return NotFound();
            }

            shuttle.IsActive = true;
            _context.SaveChanges();

            _logger.LogInformation("Shuttle unarchived successfully for BusId: {BusId}", id);
            return RedirectToAction("ViewAll", new { viewArchived = true });
        }

        private void LoadDrivers()
        {
            _logger.LogInformation("LoadDrivers called.");

            DriverServices ds = new DriverServices(_context);

            ViewBag.Drivers = ds.GetAllEntities()
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.DriverId.ToString()
                });

            _logger.LogInformation("LoadDrivers completed.");
        }
    }
}