using System.ComponentModel.DataAnnotations;

namespace TechWrite.Web.Models;

public class ContactSubmission
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Message { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public bool EmailSent { get; set; }
}
