using System.Net;
using System.Net.Mail;

namespace IdentityServer.Services;

public interface IEmailService
{
    Task SendEmailAsync(string fromAddress, string toAddress, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ISmtpSettings _smtpSettings;

    public EmailService(ISmtpSettings smtpSettings)
    {
        _smtpSettings = smtpSettings ?? throw new ArgumentNullException(nameof(smtpSettings));
    }

    /// <summary>
    /// Mailtrap's fake smtp to simulate sending the emails
    /// </summary>
    /// <param name="fromAddress"></param>
    /// <param name="toAddress"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    public async Task SendEmailAsync(string fromAddress, string toAddress, string subject, string body)
    {
        var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
            EnableSsl = true
        };

        using var message = new MailMessage(fromAddress, toAddress);
        message.Subject = subject;
        message.Body = body;
        await client.SendMailAsync(message);
    }
}