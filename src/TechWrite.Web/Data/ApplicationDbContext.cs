using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechWrite.Web.Models;

namespace TechWrite.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContactSubmission> ContactSubmissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ContactSubmission>(entity =>
        {
            entity.HasIndex(e => e.SubmittedAt);
            entity.HasIndex(e => e.IpAddress);
        });
    }
}
