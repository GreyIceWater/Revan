using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MidStateShuttleService.Models;
using MidStateShuttleService.Services;

namespace MidStateShuttleService.Controllers
{
    [Authorize(Roles = "Admin,Driver")]
    public class MailController : Controller
    {
        private readonly MailServices _mailServices;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MailController> _logger;

        public MailController(MailServices mailServices, ApplicationDbContext context, ILogger<MailController> logger)
        {
            _mailServices = mailServices;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Mail Create page accessed.");

            LoadLocations();
            return View(new MailItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MailItem mailItem)
        {
            _logger.LogInformation("Mail Create POST received.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Mail Create POST failed validation.");

                LoadLocations();
                return View(mailItem);
            }

            mailItem.SubmittedAt = DateTime.UtcNow;
            mailItem.SubmittedBy = User.Identity?.Name ?? "Unknown";

            _mailServices.AddMailItem(mailItem);

            _logger.LogInformation("Mail entry recorded successfully by {SubmittedBy}.", mailItem.SubmittedBy);

            TempData["SuccessMessage"] = "Mail entry recorded successfully.";
            return RedirectToAction(nameof(Create));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Report()
        {
            _logger.LogInformation("Mail Report accessed.");

            var mailItems = _mailServices.GetAllMailItems();

            _logger.LogInformation("Mail Report returned {MailItemCount} records.", mailItems.Count());

            return View(mailItems);
        }

        private void LoadLocations()
        {
            _logger.LogInformation("Loading active locations for mail form.");

            ViewBag.Locations = _context.Locations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new SelectListItem
                {
                    Value = l.Name,
                    Text = l.Name
                })
                .ToList();
        }
    }
}