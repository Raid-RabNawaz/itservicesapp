using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ITServicesApp.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
        Task SendBulkAsync(IEnumerable<string> toEmails, string subject, string htmlBody, CancellationToken ct = default);
    }
}
