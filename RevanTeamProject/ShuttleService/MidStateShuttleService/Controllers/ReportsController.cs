using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.Services;

namespace MidStateShuttleService.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // FULL REPORTS PAGE (Admin Only)
        // - Full page view with Layout + CSS
        // - ALL TIME data
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Reports(string report = "")
        {
            // DEV NOTE:
            // Main reports page action.
            // Uses the "report" query string to decide which dataset to load.
            var allModels = new AllModels();

            if (string.Equals(report, "requests", StringComparison.OrdinalIgnoreCase))
            {
                // DEV NOTE:
                // Load rider request records newest first for the Requests report.
                allModels.Register = await _context.RegisterModels
                    .AsNoTracking()
                    .OrderByDescending(r => r.InsertDateTime)
                    .ToListAsync();

                // DEV NOTE:
                // Passed to the view so it knows which report table to render.
                ViewBag.ReportType = "requests";
            }
            else if (string.Equals(report, "checkins", StringComparison.OrdinalIgnoreCase))
            {
                // DEV NOTE:
                // Load check-in records newest first.
                // Include related pickup/dropoff locations so names can be shown in the view.
                allModels.CheckIn = await _context.CheckIns
                    .AsNoTracking()
                    .Include(c => c.Location)
                    .Include(c => c.DropOffLocation)
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                // DEV NOTE:
                // Stored dates are UTC, so convert them to Central before displaying.
                if (allModels.CheckIn != null)
                {
                    foreach (var checkIn in allModels.CheckIn)
                        checkIn.Date = TimeService.ConvertUtcToCentral(checkIn.Date);
                }

                ViewBag.ReportType = "checkins";
            }
            else if (string.Equals(report, "mail", StringComparison.OrdinalIgnoreCase))
            {
                // DEV NOTE:
                // Load mail records newest first for the Mail report.
                allModels.MailItems = await _context.MailItems
                    .AsNoTracking()
                    .Where(m => m.IsActive)
                    .OrderByDescending(m => m.SubmittedAt)
                    .ToListAsync();

                // DEV NOTE:
                // Passed to the view so it knows which report table to render.
                ViewBag.ReportType = "mail";
            }
            else
            {
                // DEV NOTE:
                // No report selected yet, so the page loads with just the picker.
                ViewBag.ReportType = "";
            }

            // IMPORTANT: this is the FULL page view
            return View("~/Views/Dashboard/Reports.cshtml", allModels);
        }



        // ==========================================================
        // EXPORT RIDER REQUESTS (CSV) - Admin Only, ALL TIME
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ExportRiderRequestsCsv()
        {
            // DEV NOTE:
            // Pull only the fields needed for CSV export.
            var rows = await _context.RegisterModels
                .AsNoTracking()
                .OrderByDescending(r => r.InsertDateTime)
                .Select(r => new
                {
                    r.RegistrationId,
                    r.Name,
                    r.StudentId,
                    r.Email,
                    r.Phone,
                    r.IsAdult,
                    r.isCustom,
                    r.IsFieldTrip,
                    r.IsInternalInquiry,
                    r.InsertDateTime,
                    r.IsArchived
                })
                .ToListAsync();

            // DEV NOTE:
            // Build CSV content manually with a StringBuilder.
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RegistrationId,Name,StudentId,Email,Phone,IsAdult,RequestType,IsFieldTrip,IsInternalInquiry,InsertDateTime,IsArchived");

            foreach (var r in rows)
            {
                // DEV NOTE:
                // Csv(...) safely escapes text values that may contain commas, quotes, or line breaks.
                sb.AppendLine(
                    $"{r.RegistrationId}," +
                    $"{Csv(r.Name)}," +
                    $"{CsvExcelText(r.StudentId)}," +
                    $"{Csv(r.Email)}," +
                    $"{CsvExcelText(r.Phone)}," +
                    $"{r.IsAdult}," +
                    $"{Csv(r.isCustom ? "Special" : "Regular")}," +
                    $"{r.IsFieldTrip}," +
                    $"{r.IsInternalInquiry}," +
                    $"{(r.InsertDateTime.HasValue ? r.InsertDateTime.Value.ToString("MM/dd/yyyy h:mm tt") : "")}," +
                    $"{r.IsArchived}"
                );
            }



            // DEV NOTE:
            // Return the CSV as a downloadable file.
            return File(
                System.Text.Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"rider-requests-ALLTIME-{DateTime.Now:yyyyMMdd-HHmm}.csv"
            );


        }
        // DEV NOTE:
        // Excel likes to auto-format long numeric-looking values like phone numbers
        // and student IDs into scientific notation. This forces Excel to keep them as text.
        private static string CsvExcelText(string? value)
        {
            value ??= "";
            value = value.Replace("\"", "\"\"");
            return $"=\"{value}\"";
        }



        // ==========================================================
        // EXPORT CHECK-INS (CSV) - Admin Only, ALL TIME
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ExportCheckInsCsv()
        {
            // DEV NOTE:
            // Pull only the fields needed for check-in CSV export.
            var rows = await _context.CheckIns
                .AsNoTracking()
                .OrderByDescending(c => c.Date)
                .Include(c => c.Location)
                .Include(c => c.DropOffLocation)
                .Select(c => new
                {
                    c.CheckInId,
                    c.Name,
                    c.StudentId,
                    c.Date,
                    c.FirstTime,
                    PickUpLocation = c.Location != null ? c.Location.Name : "",
                    DropOffLocation = c.DropOffLocation != null ? c.DropOffLocation.Name : "",
                    c.Comments
                })
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("CheckInId,Name,StudentId,DateCentral,FirstTime,PickUpLocation,DropOffLocation,Comments");

            foreach (var c in rows)
            {
                // DEV NOTE:
                // Convert stored UTC time to Central before writing it to the CSV.
                var centralTime = TimeService.ConvertUtcToCentral(c.Date);

                sb.AppendLine(
                    $"{c.CheckInId}," +
                    $"{Csv(c.Name)}," +
                    $"{Csv(c.StudentId)}," +
                    $"{centralTime:MM/dd/yyyy h:mm tt}," +
                    $"{c.FirstTime}," +
                    $"{Csv(c.PickUpLocation)}," +
                    $"{Csv(c.DropOffLocation)}," +
                    $"{Csv(c.Comments)}"
                );
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"checkins-ALLTIME-{DateTime.Now:yyyyMMdd-HHmm}.csv"
            );
        }



        // ==========================================================
        // EXPORT MAIL (CSV) - Admin Only, ALL TIME
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ExportMailCsv()
        {
            // DEV NOTE:
            // Pull only the fields needed for mail CSV export.
            var rows = await _context.MailItems
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.SubmittedAt)
                .Select(m => new
                {
                    m.MailItemId,
                    m.SenderName,
                    m.RecipientName,
                    m.PickupLocation,
                    m.DropoffLocation,
                    m.MailType,
                    m.TrackingNumber,
                    m.Notes,
                    m.SubmittedBy,
                    m.SubmittedAt
                })
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("MailItemId,SenderName,RecipientName,PickupLocation,DropoffLocation,MailType,TrackingNumber,Notes,SubmittedBy,SubmittedAt");

            foreach (var m in rows)
            {
                sb.AppendLine(
                    $"{m.MailItemId}," +
                    $"{Csv(m.SenderName)}," +
                    $"{Csv(m.RecipientName)}," +
                    $"{Csv(m.PickupLocation)}," +
                    $"{Csv(m.DropoffLocation)}," +
                    $"{Csv(m.MailType)}," +
                    $"{Csv(m.TrackingNumber)}," +
                    $"{Csv(m.Notes)}," +
                    $"{Csv(m.SubmittedBy)}," +
                    $"{m.SubmittedAt:MM/dd/yyyy h:mm tt}"
                );
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"mail-ALLTIME-{DateTime.Now:yyyyMMdd-HHmm}.csv"
            );
        }



        // ==========================================================
        // CSV HELPER
        // ==========================================================
        private static string Csv(string? value)
        {
            // DEV NOTE:
            // Prevent null issues by converting null to empty string.
            value ??= "";

            // DEV NOTE:
            // CSV values must be quoted if they contain commas, quotes, or line breaks.
            var mustQuote =
                value.Contains(',') ||
                value.Contains('"') ||
                value.Contains('\n') ||
                value.Contains('\r');

            // DEV NOTE:
            // Escape quotes by doubling them.
            value = value.Replace("\"", "\"\"");

            return mustQuote ? $"\"{value}\"" : value;
        }

    }
}