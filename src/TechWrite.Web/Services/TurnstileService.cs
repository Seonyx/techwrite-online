using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TechWrite.Web.Services;

public class TurnstileService : ITurnstileService
{
    private readonly TurnstileSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TurnstileService> _logger;

    public TurnstileService(
        IOptions<TurnstileSettings> settings,
        HttpClient httpClient,
        ILogger<TurnstileService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ValidateTokenAsync(string token, string? remoteIp)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Turnstile token was empty");
            return false;
        }

        try
        {
            var formData = new Dictionary<string, string>
            {
                ["secret"] = _settings.SecretKey,
                ["response"] = token
            };

            if (!string.IsNullOrEmpty(remoteIp))
            {
                formData["remoteip"] = remoteIp;
            }

            var response = await _httpClient.PostAsync(
                _settings.VerifyUrl,
                new FormUrlEncodedContent(formData));

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TurnstileResponse>(responseContent);

            if (result?.Success == true)
            {
                _logger.LogInformation("Turnstile validation successful for IP {RemoteIp}", remoteIp);
                return true;
            }

            _logger.LogWarning(
                "Turnstile validation failed for IP {RemoteIp}. Error codes: {ErrorCodes}",
                remoteIp,
                result?.ErrorCodes != null ? string.Join(", ", result.ErrorCodes) : "none");

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Turnstile token");
            return false;
        }
    }

    private class TurnstileResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
