using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage User Roles")]
    public class UserRoleController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserRoleController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _db.UserRoles
                .Include(r => r.UserRolePermissions)
                .ThenInclude(rp => rp.UserPermission)
                .ToListAsync();

            return View(roles);
        }

        // ✅ AJAX METHOD ADDED HERE
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _db.UserRoles
                .Include(r => r.UserRolePermissions)
                .ThenInclude(rp => rp.UserPermission)
                .Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    isActive = r.IsActive,
                    createdAt = r.CreatedAt.ToString("yyyy-MM-dd"),
                    permissions = r.UserRolePermissions
                        .Select(p => p.UserPermission.Name)
                        .ToList()
                })
                .ToListAsync();

            return Json(roles);
        }

        // Create + Edit
        [HttpGet]
        public async Task<IActionResult> Manage(int? id)
        {
            var permissions = await _db.UserPermissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.Permissions = permissions;

            if (id == null)
                return View(new UserRoleViewModel());

            var role = await _db.UserRoles
                .Include(r => r.UserRolePermissions)
                .Include(x => x.Users)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return NotFound();

            var model = new UserRoleViewModel
            {
                Id = role.Id,
                Name = role.Name,
                IsActive = role.IsActive,
                SelectedPermissions = role.UserRolePermissions
                    .Select(rp => rp.PermissionId)
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(UserRoleViewModel model)
        {
            var permissions = await _db.UserPermissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
            ViewBag.Permissions = permissions;

            if (!ModelState.IsValid)
                return View(model);

            UserRole role;
            if (model.Id == 0)
            {
                role = new UserRole
                {
                    Name = model.Name,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };
                _db.UserRoles.Add(role);
                await _db.SaveChangesAsync();
            }
            else
            {
                role = await _db.UserRoles.FindAsync(model.Id);
                if (role == null)
                    return NotFound();

                role.Name = model.Name;
                role.IsActive = model.IsActive;
                await _db.SaveChangesAsync();

                var oldPerms = _db.UserRolePermissions
                    .Where(rp => rp.RoleId == role.Id);
                _db.UserRolePermissions.RemoveRange(oldPerms);
            }

            if (model.SelectedPermissions != null)
            {
                foreach (var permId in model.SelectedPermissions)
                {
                    _db.UserRolePermissions.Add(new UserRolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permId
                    });
                }
            }

            // users mapping (RoleId update)
            if (model.UsersRole != null)
            {
                foreach (var userId in model.UsersRole)
                {
                    var user = await _db.AppUsers.FindAsync(userId);
                    if (user != null)
                    {
                        user.RoleId = role.Id;
                    }
                }
            }

            await _db.SaveChangesAsync();
            TempData["ToastrSuccess"] = "User Role saved successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _db.UserRoles
                .Include(r => r.UserRolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return Json(new { success = false, message = "Role not found." });
            }

            if (role.Name == "Admin")
            {
                return Json(new { success = false, message = "Admin role cannot be deleted." });
            }

            var usersWithThisRole = await _db.AppUsers
                .Where(u => u.RoleId == id)
                .CountAsync();

            if (usersWithThisRole > 0)
            {
                return Json(new
                {
                    success = false,
                    message = $"Cannot delete role. {usersWithThisRole} users are assigned to this role."
                });
            }

            _db.UserRolePermissions.RemoveRange(role.UserRolePermissions);
            _db.UserRoles.Remove(role);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Role deleted successfully." });
        }
    }
}