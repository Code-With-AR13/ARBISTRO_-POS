using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{    
    [Permission("Manage User Roles")]  // sirf jo role manage kar sakta hai
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

                //var userRol = _db.AppUsers
                //    .Where(rp => rp.RoleId == role.Id);
                //_db.AppUsers.RemoveRange(userRol);
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

            // Check: kya koi user is role ko use kar raha hai?
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

            // Ab safe hai delete karna
            _db.UserRolePermissions.RemoveRange(role.UserRolePermissions);
            _db.UserRoles.Remove(role);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Role deleted successfully." });
        }




        //// TEMP: Seed permissions
        //public async Task<IActionResult> SeedPermissions()
        //{
        //    if (await _db.UserPermissions.AnyAsync())
        //    {
        //        return Content("Permissions already exist.");
        //    }

        //    var permissions = new List<UserPermission>
        //{
        //    // Portal area
        //    new UserPermission { Name = "Access Dashboard", Category = "Main Area" },
        //    new UserPermission { Name = "Manage Sales/Payments", Category = "Main Area" },
        //    new UserPermission { Name = "Manage POS", Category = "Main Area" },
        //    new UserPermission { Name = "Manage Kitchen", Category = "Main Area" },    

        //    // Food area
        //    new UserPermission { Name = "Manage Category", Category = "Inventory" },
        //    new UserPermission { Name = "Manage  Items", Category = "Inventory" },
        //    new UserPermission { Name = "Manage Modifiers", Category = "Inventory" },
        //    new UserPermission { Name = "Manage Ingredients", Category = "Inventory" },

        //    // Expense area
        //    new UserPermission { Name = "Manage Expense Types", Category = "Expense Area" },
        //    new UserPermission { Name = "Manage Expenses", Category = "Expense Area" },

        //    // Users area
        //    new UserPermission { Name = "Manage Users", Category = "Team Area" },
        //    new UserPermission { Name = "Manage User Roles", Category = "Team Area" },
        //    new UserPermission { Name = "Manage Customers", Category = "Team Area" },
        //    new UserPermission { Name = "Manage Employees", Category = "Team Area" },


        //    // Reports area
        //    new UserPermission { Name = "Overall Report", Category = "Reports Area" },
        //    new UserPermission { Name = "Tax Report", Category = "Reports Area" },
        //    new UserPermission { Name = "Expense Report", Category = "Reports Area" },
        //    new UserPermission { Name = "Stock Reports", Category = "Reports Area" },

        //    // Advance area
        //    new UserPermission { Name = "Import And Exports", Category = "Advance Area" },
        //    new UserPermission { Name = "Manage Service Tables", Category = "Advance Area" },
        //    new UserPermission { Name = "Manage Payment Methods", Category = "Advance Area" },
        //    new UserPermission { Name = "Manage Pickup Points", Category = "Advance Area" },
        //    new UserPermission { Name = "Database Backup", Category = "Advance Area" }, 


        //    // Configuration area
        //    new UserPermission { Name = "General Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Appearance Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Localization Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Ordering Meal Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Currency Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Authentication Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Captcha Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Captive Configuration", Category = "Configuration area" },
        //    new UserPermission { Name = "Printer Configuration", Category = "Configuration area" }
        //};

        //    _db.UserPermissions.AddRange(permissions);
        //    await _db.SaveChangesAsync();

        //    return Content("Permissions seeded.");
        //}
    }
}
