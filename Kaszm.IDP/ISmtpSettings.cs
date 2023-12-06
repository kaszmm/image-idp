namespace IdentityServer;

public interface ISmtpSettings
{
    string Host { get; set; }
    int Port { get; set; }
    string UserName { get; set; }
    string Password { get; set; }
}

public class SmtpSettings : ISmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}