using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserManagementApp.Data
{
    // IMPORTANT: implementing IDataProtectionKeyContext lets this DbContext
    // also store the app's Data Protection encryption keys in the database,
    // so they survive container restarts on Render's free tier (which wipes
    // the local filesystem on every restart/redeploy).
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // IMPORTANT: this DbSet is required by the IDataProtectionKeyContext interface.
        // It is the table where the encryption keys themselves get stored.
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // IMPORTANT: this is a UNIQUE INDEX on the Email column - a database-level
            // constraint separate from the Primary Key (Id), guaranteeing no two rows
            // can ever have the same email, even under concurrent inserts.
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}