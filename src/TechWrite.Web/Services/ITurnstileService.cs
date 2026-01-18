namespace TechWrite.Web.Services;

public interface ITurnstileService
{
    Task<bool> ValidateTokenAsync(string token, string? remoteIp);
}
