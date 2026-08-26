using Microsoft.AspNetCore.Identity;

namespace UserManagementApp.Data
{
    // enum means we pick one option from a fixed list
    public enum UserStatus
    {
        Unverified,
        Active,
        Blocked
    }

    // We are inheriting from IdentityUser (the built-in base class)
    // This gives us Email, PasswordHash, etc. for free
    // Then we add our own extra fields below
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserStatus Status { get; set; } = UserStatus.Unverified;
        public DateTime? LastLoginTime { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}