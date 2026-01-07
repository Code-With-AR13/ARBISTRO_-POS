using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;


namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Users")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: AppUsers
        public async Task<IActionResult> Index()
        {
            var users = await _context.AppUsers.ToListAsync();
            return View(users);
        }

        // GET: AppUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser model, string password)
        {
            // "password" ye woh plain text password hai jo tum form me lo ge
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("PasswordHash", "Password is required.");
                return View(model);
            }

            //// password ko hash + salt karo
            //CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            //model.PasswordHash = passwordHash;
            //model.PasswordSalt = passwordSalt;
            // password ko hash + salt karo (PBKDF2)
            PasswordHasher.CreateHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            model.PasswordHash = passwordHash;
            model.PasswordSalt = passwordSalt;

            model.CreatedAtUtc = DateTime.UtcNow;

            _context.AppUsers.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }
       
        // GET: AppUsers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: AppUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppUser model, string? newPassword)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            // Update basic fields
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.UpdatedAtUtc = DateTime.UtcNow;

            // Image: if new file uploaded, replace old path
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "users");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // optionally: delete old image file here

                user.UserImage = "/uploads/users/" + fileName;
            }

            //// Password: change only if newPassword provided
            //if (!string.IsNullOrWhiteSpace(newPassword))
            //{
            //    CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
            //    user.PasswordHash = passwordHash;
            //    user.PasswordSalt = passwordSalt;
            //}
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                PasswordHasher.CreateHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }


            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.AppUsers.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, message = "User not found" });

            // delete image
            if (!string.IsNullOrEmpty(item.UserImage))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    item.UserImage.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.AppUsers.Remove(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "User deleted successfully" });
        }
        //// ⚠️ TEMPORARY: only for seeding one admin user
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> SeedAdmin()
        //{
        //    // 1) Agar already admin user hai, to kuch na karo
        //    var existing = await _context.AppUsers
        //        .FirstOrDefaultAsync(u => u.Email == "admin@local.test");
        //    if (existing != null)
        //        return Content("Admin already exists");

        //    var user = new AppUser
        //    {
        //        FullName = "Super Admin",
        //        Email = "admin@local.test",
        //        Role = "Admin",
        //        IsActive = true,
        //        CreatedAtUtc = DateTime.UtcNow
        //    };

        //    // 2) Yahan tumhari PasswordHasher (PBKDF2) use karo
        //    var plainPassword = "Admin@123"; // jo chaho strong password rakho
        //    PasswordHasher.CreateHash(plainPassword, out byte[] hash, out byte[] salt);
        //    user.PasswordHash = hash;
        //    user.PasswordSalt = salt;

        //    _context.AppUsers.Add(user);
        //    await _context.SaveChangesAsync();

        //    return Content("Seeded admin: admin@local.test / Admin@123");
        //}
    }
}
