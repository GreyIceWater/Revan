using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidStateShuttleService.Models;
using MidStateShuttleService.Services;

namespace MidStateShuttleService.Controllers
{
    [Authorize(Roles = "Admin,Driver")]
    public class MailController : Controller
    {
        private readonly MailServices _mailServices;

        public MailController(MailServices mailServices)
        {
            _mailServices = mailServices;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MailItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MailItem mailItem)
        {
            if (!ModelState.IsValid)
            {
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
    }
}