using Microsoft.AspNetCore.Identity;
using UserManagementApp.Data;

namespace UserManagementApp.Middleware
{
    // This middleware runs on every single request that comes into the app.
    // Its job: check if the currently logged-in user has been blocked or deleted.
    // If so, we sign them out immediately and send them to the login page.
    // This satisfies the task requirement that blocked/deleted users get kicked out
    // even if they were already logged in when it happened.
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // We skip this check for login/register pages and static files.
            // Otherwise, a logged-out user could never even reach the login page.
            bool isExempt =
                path.StartsWith("/identity") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/favicon");

            if (context.User.Identity != null && context.User.Identity.IsAuthenticated && !isExempt)
            {
                var user = await userManager.GetUserAsync(context.User);

                // user is null means the account was deleted
                // user.Status == Blocked means someone blocked this account
                if (user == null || user.Status == UserStatus.Blocked)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}