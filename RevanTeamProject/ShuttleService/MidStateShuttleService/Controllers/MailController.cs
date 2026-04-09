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

        public MailController(MailServices mailServices, ApplicationDbContext context)
        {
            _mailServices = mailServices;
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadLocations();
            return View(new MailItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MailItem mailItem)
        {
            if (!ModelState.IsValid)
            {
                LoadLocations();
                return View(mailItem);
            }

            mailItem.SubmittedAt = DateTime.UtcNow;
            mailItem.SubmittedBy = User.Identity?.Name ?? "Unknown";

            _mailServices.AddMailItem(mailItem);

            TempData["SuccessMessage"] = "Mail entry recorded successfully.";
            return RedirectToAction(nameof(Create));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Report()
        {
            var mailItems = _mailServices.GetAllMailItems();
            return View(mailItems);
        }

        private void LoadLocations()
        {
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