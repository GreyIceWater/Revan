using System.Net;
using System.Net.Mail;

namespace MidStateShuttleService.Services
{
    public class EmailServices
    {
        private readonly IConfiguration _config;

        public EmailServices(IConfiguration config)
        {
            _config = config;
        }
        public void SendEmail(string recipient, string subject, string body, bool isHtml = false)
        {
            // Establish SMTP2GO Client
            SmtpClient client = new SmtpClient("mail.smtp2go.com")
            {
                Port = 2525, // SMTP2GO port number
                Credentials = new NetworkCredential(_config["Email:Username"],_config["Email:Password"]),
                EnableSsl = true // SMTP2GO requires SSL
            };

            MailMessage message = new MailMessage("shuttle@mstc.edu", recipient)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            // Send the email asynchronously
            client.SendMailAsync(message);
        }
        public void SendEmailToAdmin(string subject, string body, bool isHtml)
        {
            try
            {
                string adminEmail = _config["AdminSettings:AdminEmail"];

                SendEmail(adminEmail, subject, body, isHtml);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
