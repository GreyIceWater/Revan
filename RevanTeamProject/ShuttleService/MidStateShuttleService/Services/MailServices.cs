using MidStateShuttleService.Models;
using System.Collections.Generic;
using System.Linq;

namespace MidStateShuttleService.Services
{
    public class MailServices
    {
        private readonly ApplicationDbContext _context;

        public MailServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MailItem> GetAllMailItems()
        {
            return _context.MailItems
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.SubmittedAt)
                .ToList();
        }

        public MailItem? GetMailItemById(int id)
        {
            return _context.MailItems
                .FirstOrDefault(m => m.MailItemId == id && m.IsActive);
        }

        public void AddMailItem(MailItem mailItem)
        {
            _context.MailItems.Add(mailItem);
            _context.SaveChanges();
        }
    }
}