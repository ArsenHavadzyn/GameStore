using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace GameStore.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly string _smtpUser = "officegamestore@gmail.com";
        private readonly string _smtpPass = "uzcpddqoiszskuzt";
        private readonly int _smtpPort = 587;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_smtpUser),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(email);

            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                client.EnableSsl = true;
                await client.SendMailAsync(message);
            }

            Console.WriteLine($"Email to {email}: {subject} - {message}");
            await Task.CompletedTask;
        }
    }
}
