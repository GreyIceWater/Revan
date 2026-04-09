using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.Services;
using MidStateShuttleService.Helpers;

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
                    .OrderByDescending(registerModel => registerModel.InsertDateTime)
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
                    .Include(checkIn => checkIn.Location)
                    .Include(checkIn => checkIn.DropOffLocation)
                    .OrderByDescending(checkIn => checkIn.Date)
                    .ToListAsync();

                ViewBag.ReportType = "checkins";
            }
            else if (string.Equals(report, "mail", StringComparison.OrdinalIgnoreCase))
            {
                // DEV NOTE:
                // Load active mail records newest first for the Mail report.
                allModels.MailItems = await _context.MailItems
                    .AsNoTracking()
                    .Where(mailItem => mailItem.IsActive)
                    .OrderByDescending(mailItem => mailItem.SubmittedAt)
                    .ToListAsync();

                ViewBag.ReportType = "mail";
            }
            else
            {
                // DEV NOTE:
                // No report selected yet, so the page loads with just the picker.
                ViewBag.ReportType = "";
            }

            // IMPORTANT:
            // This returns the full Reports page that lives under the Dashboard views folder.
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
            var riderRequests = await _context.RegisterModels
                .AsNoTracking()
                .OrderByDescending(registerModel => registerModel.InsertDateTime)
                .Select(registerModel => new
                {
                    registerModel.RegistrationId,
                    registerModel.Name,
                    registerModel.StudentId,
                    registerModel.Email,
                    registerModel.Phone,
                    registerModel.IsAdult,
                    registerModel.isCustom,
                    registerModel.IsFieldTrip,
                    registerModel.IsInternalInquiry,
                    registerModel.InsertDateTime,
                    registerModel.IsArchived
                })
                .ToListAsync();

            // DEV NOTE:
            // Build CSV content manually with a StringBuilder.
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine("RegistrationId,Name,StudentId,Email,Phone,IsAdult,RequestType,IsFieldTrip,IsInternalInquiry,InsertDateTime,IsArchived");

            foreach (var riderRequest in riderRequests)
            {
                // DEV NOTE:
                // Csv(...) safely escapes text values that may contain commas, quotes, or line breaks.
                stringBuilder.AppendLine(
                    $"{riderRequest.RegistrationId}," +
                    $"{Csv(riderRequest.Name)}," +
                    $"{CsvExcelText(riderRequest.StudentId)}," +
                    $"{Csv(riderRequest.Email)}," +
                    $"{CsvExcelText(riderRequest.Phone)}," +
                    $"{BoolToYesNo(riderRequest.IsAdult)}," +
                    $"{Csv(riderRequest.isCustom ? "Special" : "Regular")}," +
                    $"{BoolToYesNo(riderRequest.IsFieldTrip)}," +
                    $"{BoolToYesNo(riderRequest.IsInternalInquiry)}," +
                    $"{DateTimeHelper.ToCentralTimeString(riderRequest.InsertDateTime)}," +
                    $"{BoolToYesNo(riderRequest.IsArchived)}"
                );
            }

            // DEV NOTE:
            // Return the CSV as a downloadable file.
            return File(
                System.Text.Encoding.UTF8.GetBytes(stringBuilder.ToString()),
                "text/csv",
                $"rider-requests-ALLTIME-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv"
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
            var checkIns = await _context.CheckIns
                .AsNoTracking()
                .OrderByDescending(checkIn => checkIn.Date)
                .Include(checkIn => checkIn.Location)
                .Include(checkIn => checkIn.DropOffLocation)
                .Select(checkIn => new
                {
                    checkIn.CheckInId,
                    checkIn.Name,
                    checkIn.StudentId,
                    checkIn.Date,
                    checkIn.FirstTime,
                    PickUpLocation = checkIn.Location != null ? checkIn.Location.Name : "",
                    DropOffLocation = checkIn.DropOffLocation != null ? checkIn.DropOffLocation.Name : "",
                    checkIn.Comments
                })
                .ToListAsync();

            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine("CheckInId,Name,StudentId,Date,FirstTime,PickUpLocation,DropOffLocation,Comments");

            foreach (var checkIn in checkIns)
            {
                stringBuilder.AppendLine(
                    $"{checkIn.CheckInId}," +
                    $"{Csv(checkIn.Name)}," +
                    $"{Csv(checkIn.StudentId)}," +
                    $"{DateTimeHelper.ToCentralTimeString(checkIn.Date)}," +
                    $"{BoolToYesNo(checkIn.FirstTime)}," +
                    $"{Csv(checkIn.PickUpLocation)}," +
                    $"{Csv(checkIn.DropOffLocation)}," +
                    $"{Csv(checkIn.Comments)}"
                );
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(stringBuilder.ToString()),
                "text/csv",
                $"checkins-ALLTIME-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv"
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
            var mailItems = await _context.MailItems
                .AsNoTracking()
                .Where(mailItem => mailItem.IsActive)
                .OrderByDescending(mailItem => mailItem.SubmittedAt)
                .Select(mailItem => new
                {
                    mailItem.MailItemId,
                    mailItem.DriverName,
                    mailItem.PickupLocation,
                    mailItem.DropoffLocation,
                    mailItem.MailType,
                    mailItem.Notes,
                    mailItem.SubmittedBy,
                    mailItem.SubmittedAt
                })
                .ToListAsync();

            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine("MailItemId,DriverName,PickupLocation,DropoffLocation,MailType,Notes,SubmittedBy,SubmittedAt");

            foreach (var mailItem in mailItems)
            {
                stringBuilder.AppendLine(
                    $"{mailItem.MailItemId}," +
                    $"{Csv(mailItem.DriverName)}," +
                    $"{Csv(mailItem.PickupLocation)}," +
                    $"{Csv(mailItem.DropoffLocation)}," +
                    $"{Csv(mailItem.MailType.ToString())}," +
                    $"{Csv(mailItem.Notes)}," +
                    $"{Csv(mailItem.SubmittedBy)}," +
                    $"{DateTimeHelper.ToCentralTimeString(mailItem.SubmittedAt)}"
                );
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(stringBuilder.ToString()),
                "text/csv",
                $"mail-ALLTIME-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv"
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

        // DEV NOTE:
        // Converts bool values into user-friendly Yes/No text for reports and exports.
        private static string BoolToYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}