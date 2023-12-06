using System.Net;
using System.Net.Mail;

namespace IdentityServer.Services;


public interface IEmailService
{
    Task SendEmailAsync(string fromAddress, string toAddress, string subject, string body);
}

public class EmailService : IEmailService
{
    /// <summary>
    /// Mailtrap's fake smtp to simulate sending the emails
    /// </summary>
    /// <param name="fromAddress"></param>
    /// <param name="toAddress"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    public async Task SendEmailAsync(string fromAddress, string toAddress, string subject, string body)
    {
        var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
        {
            Credentials = new NetworkCredential("d443bb383eb9a5", "********44ad"),
            EnableSsl = true
        };

        using var message = new MailMessage(fromAddress, toAddress);
        message.Subject = subject;
        message.Body = body;
        await client.SendMailAsync(message);
    }
}