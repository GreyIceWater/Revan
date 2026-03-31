using MidStateShuttleService.ViewModels;

namespace MidStateShuttleService.Models
{
    public class AllModels
    {
        public IEnumerable<CommuncateModel> Communcate { get; set; }

        public IEnumerable<Location> Location { get; set; }

        public IEnumerable<Message> Message { get; set; }

        public IEnumerable<RegisterModel> Register { get; set; }

        //public ReservationModel Reservation { get; set; }

        public IEnumerable<Routes> Route { get; set; }

        public IEnumerable<Driver> Driver { get; set; }

        public IEnumerable<Bus> Bus { get; set; }

        public IEnumerable<CheckIn> CheckIn { get; set; }

        public IEnumerable<Feedback> Feedback { get; set; }

        public IEnumerable<MailItem> MailItems { get; set; }
    }
}
