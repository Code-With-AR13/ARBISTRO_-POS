using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.Services;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Web;

namespace ARBISTO_POS.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] 
    public class AuthApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly Services.IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthApiController(
            ApplicationDbContext db,
            Services.IEmailSender emailSender,
            IConfiguration configuration)
        {
            _db = db;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        // ************ LOGIN ************
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null ||
                !user.IsActive ||
                !PasswordHasher.Verify(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

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

            if (userRole?.UserRolePermissions != null)
            {
                foreach (var rp in userRole.UserRolePermissions)
                {
                    claims.Add(new Claim("Permission", rp.UserPermission.Name));
                }
            }

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                 ? DateTimeOffset.UtcNow.AddHours(24) // ✅ Remember Me = 24 hours
                 : null
            };

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id),
                authProps);

            return Ok(new
            {
                message = $"Welcome, {user.FullName.Split(' ')[0]}!",
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    Role = user.Role,
                    Permissions = claims
                        .Where(c => c.Type == "Permission")
                        .Select(c => c.Value)
                        .ToList()
                }
            });
        }

        // ************ LOGOUT ************
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok(new { message = "Signed out." });
        }

        // ************ CURRENT USER INFO ************
        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var permissions = User.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Name = User.Identity?.Name,
                Email = User.FindFirstValue(ClaimTypes.Email),
                Role = User.FindFirstValue(ClaimTypes.Role),
                Permissions = permissions
            });
        }

        // ************ FORGOT PASSWORD ************
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            // Security best-practice: hamesha 200 return karo, detail leak mat karo. [web:6][web:9]
            if (user == null)
                return Ok(new { message = "If an account exists for this email, a reset link has been sent." });

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var expiry = DateTime.UtcNow.AddHours(2);

            user.PasswordResetToken = token;
            user.TokenExpiry = expiry;
            await _db.SaveChangesAsync();

            var baseUrl = _configuration["AppSettings:BaseUrl"]
                          ?? $"{Request.Scheme}://{Request.Host}";
            var resetLink =
                $"{baseUrl}/Auth/ResetPassword?email={HttpUtility.UrlEncode(user.Email)}&token={HttpUtility.UrlEncode(token)}";

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

            await _emailSender.SendEmailAsync(user.Email, "Password Reset - ARBISTO POS", emailBody);

            return Ok(new { message = "If an account exists for this email, a reset link has been sent." });
        }

        // ************ RESET PASSWORD (API) ************
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null ||
                user.PasswordResetToken != model.Token ||
                user.TokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired password reset link." });
            }

            PasswordHasher.CreateHash(model.Password, out var newHash, out var newSalt);

            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            user.PasswordResetToken = null;
            user.TokenExpiry = null;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Your password has been reset successfully." });
        }
    }

    // ************ DTOs ************
    public class LoginRequestDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool RememberMe { get; set; }
    }
}
