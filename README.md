# User Management Application

A clean, web-based user management admin panel built with ASP.NET Core MVC and PostgreSQL. The app lets admins view, block, unblock, and delete registered users in bulk while maintaining session safety and flexible authentication options.

## Key Features

* **User Administration Panel:** 
  * View users sorted by their recent login time.
  * Batch operations for blocking, unblocking, and hard-deleting users.
  * Instant auto-logout if a logged-in user blocks or deletes their own account.
  * Dedicated button to purge all unverified user accounts.
* **Flexible Authentication:** 
  * Custom `ApplicationUser` model tracking full name, account status, and login timestamps.
  * Simplified password rules (accepts any non-empty password) and allows login for unconfirmed accounts.
  * Custom middleware for dynamic account status checks.
* **Email & Security:** 
  * Integrated Gmail SMTP service for system emails.
  * Antiforgery data protection keys stored directly in the database to prevent session invalidation on cloud restarts.
* **Cloud Ready:** 
  * Configured for PostgreSQL with automatic database migration on startup.
  * Optimized for containerized deployments (like Render) behind reverse proxies.

## Tech Stack

* **Framework:** ASP.NET Core 8.0 MVC
* **Database:** PostgreSQL (EF Core / Npgsql)
* **Auth:** ASP.NET Core Identity
* **Hosting Target:** Render / Docker

## Quick Setup

1. **Clone & Configure:**
   Set your database connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=UserDb;Username=postgres;Password=yourpassword"
     }
   }
   ```

2. **Run Application:**
   ```bash
   dotnet run
   ```
   *Note: EF Core migrations run automatically on startup.*
