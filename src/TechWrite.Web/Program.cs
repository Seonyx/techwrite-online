using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechWrite.Web.Data;
using TechWrite.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Entity Framework Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Convert PostgreSQL URI format (from Render) to ADO.NET format if needed
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    connectionString = ConvertPostgresUri(connectionString);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// Configure contact form services
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<TurnstileSettings>(builder.Configuration.GetSection("Turnstile"));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient<ITurnstileService, TurnstileService>();
builder.Services.AddSingleton<IRateLimitService, RateLimitService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Health check endpoint for Docker/Render
app.MapGet("/health", () => Results.Ok("healthy"));

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();

// Helper function to convert PostgreSQL URI to ADO.NET connection string
static string ConvertPostgresUri(string uri)
{
    var uriObj = new Uri(uri);
    var userInfo = uriObj.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    var host = uriObj.Host;
    var port = uriObj.Port > 0 ? uriObj.Port : 5432;
    var database = uriObj.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}
