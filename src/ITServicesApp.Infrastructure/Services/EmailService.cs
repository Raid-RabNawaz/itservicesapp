using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ITServicesApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
            => SendBulkAsync(new[] { toEmail }, subject, htmlBody, ct);

        public async Task SendBulkAsync(IEnumerable<string> toEmails, string subject, string htmlBody, CancellationToken ct = default)
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];
            var from = _config["Smtp:From"] ?? user;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, pass)
            };

            using var mm = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            foreach (var to in toEmails.Distinct()) mm.To.Add(to);

            await client.SendMailAsync(mm, ct);
        }
    }
}
