using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Models;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Providers
{
    public class MailtrapProvider : IEmailProvider
    {
        public string Name => "mailtrap";

        public async Task<EmailSendResult> SendAsync(EmailData email, CancellationToken cancellationToken = default)
        {
            //using var client = new SmtpClient("smtp.gmail.com", 465)
            //{
            //    Credentials = new NetworkCredential("norouzi.hosein@gmail.com", "**********"),
            //    EnableSsl = true
            //};

            //using var message = new MailMessage
            //{
            //    From = new MailAddress("norouzi.hosein@gmail.com"/*email.From*/),
            //    Subject = (email.Subject ?? string.Empty).Replace("\r", "").Replace("\n", "").Trim(),
            //    Body = email.Body,
            //    IsBodyHtml = true 
            //};
            //message.To.Add(email.To);
            //await client.SendMailAsync(message);

            await Task.Yield();

            // A mail subject must be a single line: remove CR/LF and trim
            static string CleanSubject(string subject) =>
                System.Text.RegularExpressions.Regex.Replace(subject ?? "", @"\s+", " ").Trim();

            using (var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("norouzi.hosein@gmail.com", "sknxjjucbrmoiyzo"),
                EnableSsl = true
            })
            
            using (var message = new MailMessage
            {
                From = new MailAddress(email.From),
                Subject = CleanSubject(email.Subject),
                Body = email.Body,
                IsBodyHtml = true
            })
            {
                message.To.Add(email.To);
                client.Send(message);
            }

            return new EmailSendResult()
            {
                IsSuccess = true
            };
        }
    }
}
