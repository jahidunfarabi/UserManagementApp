using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Data;

namespace UserManagementApp.Controllers
{
    // [Authorize] means only logged-in users can reach any action in this controller.
    // Anonymous users get redirected to the login page automatically.
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // This is the main page - shows the table of all users
        public async Task<IActionResult> Index()
        {
            // Task requirement: sort by LastLoginTime
            var users = await _context.Users
                .OrderByDescending(u => u.LastLoginTime)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "No users were selected.";
                return RedirectToAction("Index");
            }

            var usersToBlock = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            foreach (var user in usersToBlock)
            {
                user.Status = UserStatus.Blocked;
            }

            await _context.SaveChangesAsync();

            // IMPORTANT: if the current logged-in user just blocked themselves
            // (or blocked "everyone" including themselves), sign them out right away
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId != null && ids.Contains(currentUserId))
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            TempData["StatusMessage"] = $"{usersToBlock.Count} user(s) blocked.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "No users were selected.";
                return RedirectToAction("Index");
            }

            var usersToUnblock = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            foreach (var user in usersToUnblock)
            {
                // Only change status if they were actually Blocked
                if (user.Status == UserStatus.Blocked)
                {
                    user.Status = UserStatus.Active;
                }
            }

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"{usersToUnblock.Count} user(s) unblocked.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "No users were selected.";
                return RedirectToAction("Index");
            }

            var usersToDelete = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            // Task requirement: this is a real hard delete, not just marking a flag.
            // A deleted user should be able to register again with the same email later.
            _context.Users.RemoveRange(usersToDelete);
            await _context.SaveChangesAsync();

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId != null && ids.Contains(currentUserId))
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            TempData["StatusMessage"] = $"{usersToDelete.Count} user(s) deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnverified()
        {
            var unverifiedUsers = await _context.Users
                .Where(u => u.Status == UserStatus.Unverified)
                .ToListAsync();

            _context.Users.RemoveRange(unverifiedUsers);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"{unverifiedUsers.Count} unverified user(s) deleted.";
            return RedirectToAction("Index");
        }
    }
}