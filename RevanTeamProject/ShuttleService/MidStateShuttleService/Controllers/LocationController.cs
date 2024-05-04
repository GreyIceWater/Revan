﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using System.Data;

namespace MidStateShuttleService.Controllers
{
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocationController> _logger;

        // Inject ApplicationDbContext and ILogger into the controller constructor
        public LocationController(ApplicationDbContext context, ILogger<LocationController> logger)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            _logger = logger; // Assign the injected ILogger to the _logger field
        }

        // GET: LocationController
        public ActionResult Index()
        {
            return View();
        }

        // GET: LocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LocationController/Create
        [HttpPost]
        public ActionResult Create(Location location)
        {
            if (!ModelState.IsValid)
            {
                return View(location);
            }

            try
            {
                LocationServices ls = new LocationServices(_context);
                location.IsActive = true;
                ls.AddEntity(location);

                TempData["SuccessMessage"] = "The location has been successfully created!";
                HttpContext.Session.SetString("LocationSuccess", "true");
                TempData["LocationSuccess"] = true;
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context); // Log SQL exception
                _logger.LogError(ex, "An error occurred while creating location.");
                ModelState.AddModelError("", "An unexpected error occurred, please try again.");
                return View(location);
            }
        }


        // GET: LocationController/Edit/5
        public ActionResult Edit(int id)
        {
            LocationServices ls = new LocationServices(_context);
            Location model = ls.GetEntityById(id);

            if (model == null)
                return FailedLocation("Check In Not Found");

            return View(model);
        }

        // POST: LocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Location model)
        {
            LocationServices ls = new LocationServices(_context);
            if (model == null)
                return FailedLocation("Updates to location could not be applied");

            try
            {
                ls.UpdateEntity(model);
                HttpContext.Session.SetString("LocationSuccess", "true");
                TempData["LocationSuccess"] = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                return FailedLocation("Updates to location could not be applied");
            }

            return RedirectToAction("Edit");
        }

        // GET: LocationController/Delete/5
        public ActionResult DeleteLocation(int id)
        {
            try
            {
                LocationServices ls = new LocationServices(_context);
                Location model = ls.GetEntityById(id);

                if (model == null)
                    return FailedLocation("Check In Not Found");

                model.IsActive = !model.IsActive; // Toggle IsActive from true to false or false to true
                ls.UpdateEntity(model); // Update the entity in the database

                return RedirectToAction("Index", "Dashboard"); // Redirect after toggling IsActive
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                return FailedLocation("Updates to location could not be applied");
            }
        }

        // POST: LocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        public ActionResult FailedLocation(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            return View("FailedLocation");
        }
    }
}
