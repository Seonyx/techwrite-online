using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TechWrite.Web.Data;
using TechWrite.Web.Models;
using TechWrite.Web.Services;

namespace TechWrite.Web.Pages;

public class ContactModel : PageModel
{
    private readonly IEmailService _emailService;
    private readonly ITurnstileService _turnstileService;
    private readonly IRateLimitService _rateLimitService;
    private readonly ApplicationDbContext _dbContext;
    private readonly TurnstileSettings _turnstileSettings;
    private readonly ILogger<ContactModel> _logger;

    public ContactModel(
        IEmailService emailService,
        ITurnstileService turnstileService,
        IRateLimitService rateLimitService,
        ApplicationDbContext dbContext,
        IOptions<TurnstileSettings> turnstileSettings,
        ILogger<ContactModel> logger)
    {
        _emailService = emailService;
        _turnstileService = turnstileService;
        _rateLimitService = rateLimitService;
        _dbContext = dbContext;
        _turnstileSettings = turnstileSettings.Value;
        _logger = logger;
    }

    [BindProperty]
    public ContactFormInput Input { get; set; } = new();

    [BindProperty]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [BindProperty]
    public long FormLoadTime { get; set; }

    [BindProperty(Name = "cf-turnstile-response")]
    public string? TurnstileResponse { get; set; }

    public string TurnstileSiteKey => _turnstileSettings.SiteKey;

    public bool ShowSuccessMessage { get; set; }
    public bool ShowErrorMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        FormLoadTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var clientIp = GetClientIpAddress();

        // Check honeypot field
        if (!string.IsNullOrWhiteSpace(Website))
        {
            _logger.LogWarning("Honeypot triggered from IP {IpAddress}", clientIp);
            ShowSuccessMessage = true;
            return Page();
        }

        // Check time-based validation (form should take at least 3 seconds to fill)
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime - FormLoadTime < 3)
        {
            _logger.LogWarning("Time-based validation failed from IP {IpAddress}. Form submitted in {Seconds}s",
                clientIp, currentTime - FormLoadTime);
            ShowSuccessMessage = true;
            return Page();
        }

        // Check rate limiting
        if (_rateLimitService.IsRateLimited(clientIp))
        {
            ShowErrorMessage = true;
            ErrorMessage = "Too many submissions. Please try again later.";
            return Page();
        }

        // Validate Turnstile
        if (!await _turnstileService.ValidateTokenAsync(TurnstileResponse ?? "", clientIp))
        {
            ShowErrorMessage = true;
            ErrorMessage = "Security verification failed. Please try again.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Record submission for rate limiting
        _rateLimitService.RecordSubmission(clientIp);

        // Save to database
        var submission = new ContactSubmission
        {
            Name = Input.Name,
            Email = Input.Email,
            Subject = Input.Subject,
            Message = Input.Message,
            IpAddress = clientIp,
            SubmittedAt = DateTime.UtcNow
        };

        try
        {
            // Send email
            var emailSent = await _emailService.SendContactEmailAsync(
                Input.Name, Input.Email, Input.Subject, Input.Message);

            submission.EmailSent = emailSent;

            _dbContext.ContactSubmissions.Add(submission);
            await _dbContext.SaveChangesAsync();

            if (emailSent)
            {
                ShowSuccessMessage = true;
                ModelState.Clear();
                Input = new ContactFormInput();
            }
            else
            {
                ShowErrorMessage = true;
                ErrorMessage = "There was a problem sending your message. Please try again or email directly.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact form submission");
            ShowErrorMessage = true;
            ErrorMessage = "An unexpected error occurred. Please try again later.";
        }

        FormLoadTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Page();
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public class ContactFormInput
    {
        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a subject")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your message")]
        [StringLength(5000, ErrorMessage = "Message cannot exceed 5000 characters")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
    }
}
