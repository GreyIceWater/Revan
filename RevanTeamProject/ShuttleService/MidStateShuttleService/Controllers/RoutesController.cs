using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using System.Data;

namespace MidStateShuttleService.Controllers
{
    public class RoutesController : Controller
    {
        private readonly ILogger<LocationController> _logger;

        private readonly ApplicationDbContext _context;
        

        // Inject ApplicationDbContext into the controller constructor
        public RoutesController(ApplicationDbContext context, ILogger<LocationController> logger)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            
            _logger = logger;
        }

        // GET: RoutesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: RoutesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RoutesController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            LocationServices ls = new LocationServices(_context);
            ViewBag.Locations = ls.GetAllEntities().Select(x => new SelectListItem { Text = x.Name, Value = x.LocationId.ToString() });

            // Assuming GetAllEntities() returns a list of drivers
            DriverServices ds = new DriverServices(_context);
            ViewBag.Drivers = ds.GetAllEntities().Select(x => new SelectListItem { Text = x.Name, Value = x.DriverId.ToString() });

            return View();
        }


        // POST: RoutesController/Edit/
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Routes route)
        {
            RouteServices rs = new RouteServices(_context);
            route.IsActive = true;
            rs.AddEntity(route);

            HttpContext.Session.SetString("RouteSuccess", "true");
            TempData["RouteSuccess"] = true;

            return RedirectToAction("Create");
        }

        // GET: RoutesController/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var route = _context.Routes.Find(id);

            if (route == null)
            {
                return NotFound();
            }

            route.PickUpTime = null;
            route.DropOffTime = null;

            LocationServices ls = new LocationServices(_context);
            ViewBag.Locations = ls.GetAllEntities().Select(x => new SelectListItem { Text = x.Name, Value = x.LocationId.ToString() });

            BusServices bs = new BusServices(_context);
            ViewBag.Buses = bs.GetAllEntities().Select(x => new SelectListItem { Text = "Shuttle: " + x.BusNo, Value = x.BusId.ToString() });

            DriverServices ds = new DriverServices(_context);
            ViewBag.Drivers = ds.GetAllEntities().Select(x => new SelectListItem { Text = x.Name, Value = x.DriverId.ToString() });

            return View(route);
        }

        // POST: RoutesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Routes updatedRoute)
        {
            if(id != updatedRoute.RouteID)
            {
                return BadRequest();
            }

            

            try
            {
                updatedRoute.IsActive = true;
                _context.Update(updatedRoute);
                _context.SaveChanges();

                HttpContext.Session.SetString("RouteSuccess", "true");
                TempData["RouteSuccess"] = true;

                TempData["SuccessMessage"] = "The route has been successfully updated!";
                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while updating the route.");
                return RedirectToAction("Index", "Dashboard");
            }
        }



        // GET: RoutesController/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                var route = _context.Routes.Find(id);

                if (route != null)
                {
                    route.IsActive = !route.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();
                }
                else
                {
                    // Handle the case where the route with the specified id is not found
                    ModelState.AddModelError("", "Route not found.");
                    return View();
                }

                return RedirectToAction("Index", "Dashboard"); // Redirect after toggling IsActive
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the route.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the route, please try again.");

                // Return the view with an error message
                return View();
            }

        }

        // POST: RoutesController/Delete/5
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
