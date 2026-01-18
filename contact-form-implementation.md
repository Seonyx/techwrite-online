# Contact Form Implementation

I need to implement a contact form for techwrite.online with anti-spam protection and email integration.

## CURRENT STATE:
- ASP.NET Core 8 with Identity framework
- PostgreSQL database
- Deployed on Render.com
- Email service: Migadu (credentials ready for environment variables)
- Anti-spam: Cloudflare Turnstile (credentials ready)

## REQUIREMENTS:

### 1. Contact Form Page (/contact)
- Name (required)
- Email (required, validated)
- Subject (required)
- Message (required, textarea)
- Honeypot field (hidden with CSS, not display:none)
- Clean, professional styling consistent with existing site

### 2. Anti-Spam Measures
- Implement Cloudflare Turnstile
- Add invisible honeypot field with time-based validation
- Rate limiting per IP address (max 3 submissions per hour)

### 3. Email Integration
- Configure SMTP with Migadu
- Send to: configured recipient address
- Professional email template with all form data
- Success/error messages to user
- Proper error handling and logging

### 4. Configuration
- Store SMTP credentials in appsettings.json structure (actual values will be added to Render environment variables)
- Store Turnstile keys in configuration
- All secrets configurable via environment variables

### 5. Navigation Updates
- Replace any visible email addresses on site with "Contact" link to /contact
- Add contact link to navigation/footer

## TECHNICAL PREFERENCES:
- Use built-in ASP.NET Core validation
- Follow existing project patterns and structure
- Include proper error handling and logging
- Make email service injectable for testability
- Use async/await patterns for email sending

## DELIVERABLES:
Please create all necessary files, update existing files as needed, and provide:
1. List of environment variables needed for Render configuration
2. Instructions for testing the contact form locally
3. Any additional setup steps required