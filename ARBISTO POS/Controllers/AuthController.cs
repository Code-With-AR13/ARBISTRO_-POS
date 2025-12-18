using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARBISTO_POS.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AuthController(ApplicationDbContext db) => _db = db;

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already logged in, go home.
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? Url.Action("Index", "Home")
                : returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
        {
            returnUrl ??= Url.Action("Index", "Home");

            var user = await _db.AppUsers.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
            {
                TempData["ToastrError"] = "Invalid email or password.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe
            };

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id),
                authProps);

            TempData["ToastrSuccess"] = $"Welcome, {user.FullName.Split(' ')[0]}!";
            return LocalRedirect(returnUrl!);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            TempData["ToastrSuccess"] = "Signed out.";
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
