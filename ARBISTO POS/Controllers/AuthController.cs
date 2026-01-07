using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.Services;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace ARBISTO_POS.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Services.IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext db,
            Services.IEmailSender emailSender,
            IConfiguration configuration)
        {
            _db = db;
            _emailSender = emailSender;
            _configuration = configuration;
        }

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

            // Pehle user load karo (bina role navigation ke)
            var user = await _db.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
            {
                TempData["ToastrError"] = "Invalid email or password.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Ab user.Role (string) se matching UserRole aur uski permissions load karo
            UserRole? userRole = null;
            if (!string.IsNullOrEmpty(user.Role))
            {
                userRole = await _db.UserRoles
                    .Include(r => r.UserRolePermissions)
                    .ThenInclude(rp => rp.UserPermission)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Name == user.Role);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role ?? "User")
    };

            // Add permissions as claims from userRole (jo string Role se match hua)
            if (userRole?.UserRolePermissions != null)
            {
                foreach (var rp in userRole.UserRolePermissions)
                {
                    claims.Add(new Claim("Permission", rp.UserPermission.Name));
                }
            }

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




        // Forgot Password GET
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Forgot Password POST
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null)
                return View("ForgotPasswordConfirmation");

            // Generate token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var expiry = DateTime.UtcNow.AddHours(2);

            user.PasswordResetToken = token;
            user.TokenExpiry = expiry;
            await _db.SaveChangesAsync();

            // Create reset link
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            var resetLink = $"{baseUrl}/Auth/ResetPassword?email={HttpUtility.UrlEncode(user.Email)}&token={HttpUtility.UrlEncode(token)}";

            // Send email
            var emailBody = $@"
    <html>
    <body style='font-family: Arial, sans-serif;'>
        <h2>Password Reset Request</h2>
        <p>Hi {user.FullName},</p>
        <p>You requested a password reset. Click the button below to set a new password:</p>
        <p><a href='{resetLink}' style='background:#0d6efd; color:#fff; padding:10px 20px; text-decoration:none; border-radius:5px;'>Reset Password</a></p>
        <p>This link will be valid for 2 hours.</p>
        <p>If you did not request this, you can safely ignore this email.</p>
    </body>
    </html>";

            
            // FINAL LINE
            await _emailSender.SendEmailAsync(user.Email, "Password Reset - ARBISTO POS", emailBody);

            return View("ForgotPasswordConfirmation");
        }

        // Reset Password GET
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid password reset request.");

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        // Reset Password POST
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null || user.PasswordResetToken != model.Token || user.TokenExpiry < DateTime.UtcNow)
            {
                TempData["ToastrError"] = "Invalid or expired password reset link.";
                return View(model);
            }

            // Hash new password
            PasswordHasher.CreateHash(model.Password, out var newHash, out var newSalt);

            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            user.PasswordResetToken = null;
            user.TokenExpiry = null;

            await _db.SaveChangesAsync();

            TempData["ToastrSuccess"] = "Your password has been reset successfully.";
            return RedirectToAction(nameof(Login));
        }

    }
}
