namespace TechWrite.Web.Services;

public interface IEmailService
{
    Task<bool> SendContactEmailAsync(string name, string email, string subject, string message);
}
