using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Using PostgreSQL now instead of SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// IMPORTANT: persist Data Protection keys in the database instead of local disk.
// On Render's free tier, the container's local filesystem is wiped on every
// restart/redeploy, which would generate a brand new encryption key each time.
// That breaks anything relying on the old key - like antiforgery tokens on
// forms that were already loaded in the browser before the restart happened.
// Storing keys in the database means they survive restarts.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Using our own ApplicationUser class instead of the default IdentityUser
// because we need extra fields like FullName, Status, LastLoginTime
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Task requires that unverified users can still log in, so we turn this off
    options.SignIn.RequireConfirmedAccount = false;

    // Task requires that any non-empty password is accepted
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Register our custom EmailSender so Identity uses real Gmail SMTP
// instead of the default fake/no-op email sender
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, UserManagementApp.Services.EmailSender>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// NOTE: HTTPS redirection & HSTS removed for production.
// Render terminates TLS at its own edge/proxy; the container itself
// only ever receives plain HTTP traffic. Keeping UseHttpsRedirection()
// here would cause redirect loops behind Render's proxy.

app.UseRouting();

// IMPORTANT: UseAuthentication must come BEFORE UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// IMPORTANT: this must run AFTER UseAuthentication/UseAuthorization
// so that context.User is already populated when our middleware checks it
app.UseMiddleware<UserManagementApp.Middleware.UserStatusMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Auto-apply any pending EF Core migrations at startup.
// Render's PostgreSQL database starts empty, so this ensures the
// schema (tables etc.) gets created automatically on first deploy
// and on every future deploy that includes new migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();