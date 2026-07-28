# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

TechWrite.Online — a technical writing portfolio and client portal for Stephen Gould, migrated from www.seonyx.com. Built with ASP.NET Core 8 Razor Pages, deployed to mywindowshosting.com (IIS, out-of-process via ANCM). ASP.NET Identity is scaffolded in but auth links are intentionally hidden pending future client portal work.

## Commands

All commands run from `src/TechWrite.Web/`.

```bash
dotnet build
dotnet run                        # http://localhost:5195
dotnet ef migrations add <Name>
dotnet ef database update
```

No test project exists yet.

## Architecture

**Stack:** ASP.NET Core 8 Razor Pages · EF Core + SQL Server · ASP.NET Identity · Bootstrap 5 · MailKit · Cloudflare Turnstile

**Entry point:** `Program.cs` wires all DI registrations and applies EF migrations on startup automatically.

**Pages** (`Pages/`) follow standard Razor Pages conventions. All public pages are complete (Home, About, Services, Portfolio, Contact, Privacy). Auth pages are scaffolded via Identity UI but links are hidden in `_Layout.cshtml`.

**Services** (`Services/`) are interface-based, injected via DI:
- `IEmailService` / `EmailService` — MailKit SMTP to Migadu (smtp.migadu.com:587, STARTTLS). Configured via `SmtpSettings` bound from `Smtp:*` config keys.
- `ITurnstileService` / `TurnstileService` — Cloudflare Turnstile verification. Configured via `TurnstileSettings` bound from `Turnstile:*` config keys.
- `IRateLimitService` / `RateLimitService` — singleton in-memory rate limiter (3 submissions/IP/hour, `ConcurrentDictionary`).

**Contact form anti-spam layers** (applied in order in `Contact.cshtml.cs`):
1. Honeypot field (hidden via CSS absolute positioning, not `display:none`)
2. Time-based check (form must take ≥3 seconds)
3. Rate limit (3/hour per IP)
4. Cloudflare Turnstile token verification
5. Model state validation
6. Save `ContactSubmission` to DB, send email

**Database:** SQL Server (`sql5063.site4now.net`, db `db_a8713c_techwrite` in production; LocalDB in development). `ApplicationDbContext` inherits `IdentityDbContext` and adds `DbSet<ContactSubmission>`. Migrations are in `Data/Migrations/`.

**Configuration:**
- `appsettings.Development.json` uses Mailhog (localhost:1025) and Turnstile test keys — no real credentials needed locally.
- Production secrets live in `appsettings.Production.json` on the server (gitignored, uploaded manually via host panel File Manager).

**Deployment:** mywindowshosting.com native "GitHub Deploy" panel feature — connects via GitHub App, builds and deploys on push to `main`. IIS out-of-process via AspNetCoreModuleV2 (`AspNetCoreHostingModel=OutOfProcess` in csproj). DataProtection keys persisted to `App_Data/dp-keys/` (survives app-pool recycles). Health check at `/health`.
