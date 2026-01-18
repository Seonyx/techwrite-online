# TechWrite.Online - Project Brief for Claude Code

## Project Overview

**Goal:** Migrate existing website from www.seonyx.com to www.techwrite.online, rebuilding it as a modern ASP.NET Core application with user authentication framework for future client portal functionality.

**Primary Objectives:**
1. Learn and master Claude Code through hands-on project development
2. Create a professional technical writing portfolio/brochure site
3. Implement authentication framework from day one for future expansion
4. Deploy to free hosting (Render.com)
5. Prepare for eventual Linux/macOS workflow (no Visual Studio dependency)

**Developer Background:**
- Experienced Unix sysadmin (comfortable with terminal/command line)
- Recent .NET experience on Windows
- Wants to work in VSCode, not Visual Studio
- Planning to migrate fully to Linux/macOS later in 2026

---

## Technical Stack

### Core Technologies
- **Framework:** ASP.NET Core 8 (latest LTS)
- **Language:** C# 12
- **Database:** PostgreSQL (Render managed instance)
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Frontend:** Razor Pages (simple, maintainable)
- **Styling:** Modern CSS (Tailwind or similar - keep it clean and professional)
- **Development Environment:** VSCode with C# Dev Kit
- **Version Control:** Git + GitHub (https://github.com/Seonyx/techwrite-online)
- **Deployment:** Docker container on Render.com
- **CI/CD:** GitHub Actions for automated deployment

### Why These Choices
- **PostgreSQL over SQL Server:** Free, excellent, works on all platforms
- **Razor Pages over MVC:** Simpler for content-focused site, easier to maintain
- **Docker:** Platform-independent, works same way locally and in production
- **Render.com:** Free tier, good .NET support, simple deployment, no Oracle nonsense

---

## Current Site Analysis (seonyx.com)

**Content to Migrate:**
- Menu structure and navigation
- Page content (text, images)
- Professional writing samples
- Contact information
- Service offerings

**What We're Moving Away From:**
- Orchard CMS (over-engineered, clumsy, complicated to edit)
- Windows-only deployment
- CMS overhead for a brochure site

**What We're Building Instead:**
- Clean, simple codebase
- Direct content editing in code/markdown
- Full control over everything
- Easy to understand and maintain

---

## Architecture & Design

### Application Structure

```
techwrite-online/
├── src/
│   └── TechWrite.Web/
│       ├── Pages/              # Razor Pages
│       │   ├── Index.cshtml
│       │   ├── About.cshtml
│       │   ├── Services.cshtml
│       │   ├── Portfolio.cshtml
│       │   ├── Contact.cshtml
│       │   └── Account/        # Auth pages
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Migrations/
│       ├── Models/
│       ├── wwwroot/            # Static files
│       │   ├── css/
│       │   ├── js/
│       │   └── images/
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Program.cs
│       └── TechWrite.Web.csproj
├── Dockerfile
├── .dockerignore
├── .gitignore
├── README.md
└── docker-compose.yml          # For local development
```

### Database Design

**Initial Schema:**
- ASP.NET Core Identity tables (built-in)
- Users, Roles, Claims (standard Identity tables)
- Future: Projects, Clients, Assignments tables (leave room for expansion)

**Migration Strategy:**
- Use EF Core migrations from day one
- Document all schema changes
- Keep migrations in version control

### Authentication & Authorization

**ASP.NET Core Identity Features:**
- User registration (initially disabled/admin-only)
- Login/logout
- Password reset
- Email confirmation (configure later)
- Two-factor authentication (enable later)
- Role-based authorization ready for future use

**Initial Roles:**
- Admin (site owner)
- Client (for future client portal)

---

## Development Phases

### Phase 1: Foundation (First Priority)
**Goal:** Get basic application running locally and deployed

1. **Local Development Setup**
   - Initialize ASP.NET Core project
   - Configure PostgreSQL connection
   - Set up Identity
   - Create basic page structure
   - Test locally with Docker Compose

2. **Deployment Pipeline**
   - Create Dockerfile
   - Configure Render.com
   - Set up PostgreSQL on Render
   - Deploy initial version
   - Verify deployment works

**Deliverables:**
- Running application on Render
- Database connected and working
- Basic homepage displaying
- Authentication system functional

### Phase 2: Content Migration
**Goal:** Move content from seonyx.com to new site

1. **Page Creation**
   - Recreate menu structure
   - Create all main pages
   - Style pages professionally
   - Ensure responsive design

2. **Content Transfer**
   - Copy text content from seonyx.com
   - Migrate/optimize images
   - Update contact information
   - Add portfolio samples

**Deliverables:**
- All pages from seonyx.com recreated
- Professional styling applied
- Mobile-responsive design
- Fast page loads

### Phase 3: Polish & Features
**Goal:** Professional finish and future-ready features

1. **User Experience**
   - Contact form (email integration) / anti-spam measures
   - Professional typography
   - Proper SEO meta tags
   - Fast, clean, accessible

2. **Admin Features**
   - Simple content editing (if needed)
   - User management interface
   - Basic analytics setup

**Deliverables:**
- Fully functional contact form
- Admin dashboard
- Analytics integrated
- Documentation for maintenance

### Phase 4: Future Expansion (Not Immediate)
**Goal:** Client portal features when needed

- Client project uploads
- Assignment tracking
- Document management
- Payment integration
- Client communication

---

## Render.com Deployment Configuration

### Services to Create

1. **Web Service (Free Tier)**
   - Type: Web Service
   - Environment: Docker
   - Repository: https://github.com/Seonyx/techwrite-online
   - Branch: main
   - Build Command: (Docker handles this)
   - Start Command: (Docker handles this)

2. **PostgreSQL Database (Free Tier)**
   - Type: PostgreSQL
   - Plan: Free (1GB storage, 90-day retention)
   - Name: techwrite-db
   - PostgreSQL Version: 16 (latest)

### Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
ConnectionStrings__DefaultConnection=<Render provides this>
```

### Custom Domain Setup (Later)
- Point techwrite.online DNS to Render
- Configure SSL (automatic with Render)

---

## Docker Configuration

### Dockerfile Requirements
- Multi-stage build (build -> runtime)
- .NET 8 SDK for building
- .NET 8 ASP.NET runtime for running
- Optimize for size and security
- Run as non-root user
- Health check endpoint

### docker-compose.yml (Local Development)
```yaml
version: '3.8'

services:
  web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=db;Database=techwrite;Username=postgres;Password=postgres
    depends_on:
      - db
    volumes:
      - ./src:/app/src

  db:
    image: postgres:16
    environment:
      - POSTGRES_PASSWORD=postgres
      - POSTGRES_DB=techwrite
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

---

## VSCode Development Setup

### Required Extensions
- C# Dev Kit
- Docker
- PostgreSQL (for database management)
- GitLens (optional but helpful)

### Launch Configuration (.vscode/launch.json)
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/TechWrite.Web/bin/Debug/net8.0/TechWrite.Web.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/TechWrite.Web",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

---

## Security Considerations

### Must-Haves from Day One
1. **Secrets Management**
   - Use User Secrets for local development
   - Use Render environment variables for production
   - Never commit connection strings or API keys

2. **HTTPS**
   - Enforce HTTPS in production (Render handles SSL)
   - HSTS headers enabled

3. **Authentication Security**
   - Strong password requirements
   - Account lockout after failed attempts
   - Secure cookie configuration
   - CSRF protection (built into Razor Pages)

4. **Database Security**
   - Parameterized queries (EF Core handles this)
   - Least-privilege database user
   - Connection string encryption

### Future Security Enhancements
- Rate limiting
- Two-factor authentication
- Security headers (CSP, X-Frame-Options, etc.)
- Regular dependency updates

---

## Content Strategy

### Initial Pages (from seonyx.com)
1. **Home** - Professional introduction, services overview
2. **About** - Background, experience, expertise
3. **Services** - Technical writing services offered
4. **Portfolio** - Writing samples and case studies
5. **Contact** - Contact form and information

### Content Priorities
- Clear, professional tone
- SEO-friendly (meta tags, semantic HTML)
- Fast loading (optimized images)
- Accessible (WCAG 2.1 AA compliance)
- Mobile-first responsive design

### Future Content Areas
- Blog (when ready)
- Client login portal
- Project dashboard
- Resource library

---

## Testing Strategy

### Local Testing
- Manual testing during development
- Test authentication flows
- Verify responsive design
- Check all links work
- Test contact form

### Deployment Testing
- Smoke test after each deployment
- Verify database migrations
- Check production environment variables
- Test HTTPS enforcement
- Verify custom domain (when configured)

### Future Testing
- Unit tests for business logic
- Integration tests for authentication
- E2E tests for critical paths

---

## Documentation Requirements

### Code Documentation
- README with setup instructions
- Architecture decision records (ADRs)
- API documentation (when needed)
- Deployment guide

### User Documentation
- Admin user guide
- Content editing guide
- Client portal guide (future)

---

## Success Criteria

### Phase 1 Success
- [ ] Application runs locally via Docker Compose
- [ ] PostgreSQL connection working
- [ ] ASP.NET Core Identity configured
- [ ] Can register and login as user
- [ ] Deployed successfully to Render.com
- [ ] Production database connected
- [ ] HTTPS working on Render

### Phase 2 Success
- [ ] All seonyx.com pages recreated
- [ ] Content migrated completely
- [ ] Professional styling applied
- [ ] Responsive on mobile/tablet/desktop
- [ ] Fast page loads (<2s)

### Phase 3 Success
- [ ] Contact form working
- [ ] Professional appearance
- [ ] SEO meta tags complete
- [ ] Ready for client use
- [ ] Documentation complete

---

## Known Constraints & Considerations

### Render.com Free Tier Limitations
- Services spin down after 15 minutes of inactivity (first request slower)
- 750 hours/month free (adequate for low-traffic site)
- 100GB bandwidth/month
- Database: 1GB storage, 90-day data retention

**Mitigation:**
- Acceptable for initial launch
- Can upgrade when client work starts
- Set up uptime monitoring (Render provides this)

### Development Environment
- Must work in VSCode (no Visual Studio dependency)
- Docker required for local development
- PostgreSQL for local dev matches production

### Future Migration Path
- Code is platform-independent (works on Linux/macOS)
- Docker ensures consistency across environments
- Easy to migrate to different hosting if needed

---

## Getting Started Checklist

### Immediate Actions for Claude Code

1. **Project Initialization**
   - [ ] Clone GitHub repository
   - [ ] Initialize .NET Core 8 project structure
   - [ ] Set up solution/project files
   - [ ] Configure .gitignore properly

2. **Core Setup**
   - [ ] Add ASP.NET Core Identity
   - [ ] Configure Entity Framework Core with PostgreSQL
   - [ ] Create initial database migrations
   - [ ] Set up basic Razor Pages structure

3. **Local Development**
   - [ ] Create Dockerfile
   - [ ] Create docker-compose.yml
   - [ ] Test local build and run
   - [ ] Verify database connection

4. **Basic Pages**
   - [ ] Create layout page
   - [ ] Create navigation
   - [ ] Create home page
   - [ ] Test authentication pages work

5. **Deployment Preparation**
   - [ ] Configure for Render deployment
   - [ ] Set up environment variables
   - [ ] Create deployment documentation
   - [ ] Test deployment process

---

## Questions for Claude Code to Resolve

As Claude Code works through this project, here are key decisions to make:

1. **CSS Framework:** Tailwind CSS, Bootstrap, or custom CSS?
2. **Email Service:** SendGrid, Mailgun, or defer for later?
3. **File Storage:** Local filesystem or object storage (for future uploads)?
4. **Logging:** Serilog, NLog, or built-in logging?
5. **Monitoring:** Application Insights, Sentry, or Render built-in?

**Recommendation:** Start simple, add complexity only when needed.

---

## Important Notes

### What Makes This Project Different
- **Learning focused:** This is about learning Claude Code and modern .NET development
- **Practical goal:** Real business need (technical writing portfolio)
- **Future-ready:** Built for expansion (client portal features)
- **Platform-agnostic:** Works on Linux/macOS/Windows via Docker

### Development Philosophy
- **Keep it simple:** Avoid over-engineering
- **Ship early:** Get something deployed quickly, iterate
- **Document as you go:** Future you will thank present you
- **Test the critical paths:** Authentication and deployment must work reliably

### When to Ask for Help
- PostgreSQL connection issues
- Render deployment problems
- Authentication configuration
- Docker build failures
- Migration conflicts

---

## Project Timeline Expectations

**Realistic Timeline:**
- Phase 1 (Foundation): 2-4 hours
- Phase 2 (Content): 2-3 hours  
- Phase 3 (Polish): 2-3 hours
- **Total:** 6-10 hours of focused work

**Not a Sprint:** This is a learning project. Take time to understand what's being built.

---

## Resources & References

### Official Documentation
- ASP.NET Core: https://docs.microsoft.com/aspnet/core/
- Entity Framework Core: https://docs.microsoft.com/ef/core/
- PostgreSQL: https://www.postgresql.org/docs/
- Render: https://render.com/docs
- Docker: https://docs.docker.com/

### Helpful Guides
- ASP.NET Core Identity: https://docs.microsoft.com/aspnet/core/security/authentication/identity
- EF Core with PostgreSQL: https://www.npgsql.org/efcore/
- Deploying .NET to Render: https://render.com/docs/deploy-dotnet

---

## Contact & Support

**Project Owner:** Developer with Unix sysadmin background, learning modern .NET and Claude Code
**Communication:** Through Claude Code interface
**Repository:** https://github.com/Seonyx/techwrite-online

---

## Final Thoughts

This project is about:
1. ✅ Learning Claude Code effectively
2. ✅ Building something useful (not a toy project)
3. ✅ Creating a platform-independent workflow
4. ✅ Establishing foundation for future business growth

**Let's build this!**

---

## Version History
- v1.0 - 2026-01-18 - Initial project brief created
