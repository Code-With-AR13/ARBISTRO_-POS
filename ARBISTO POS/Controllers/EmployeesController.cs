using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Employees")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ================= AJAX ENDPOINT =================
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var data = await _context.Employees
                .Select(e => new
                {
                    id = e.Id,
                    fullName = e.FullName,
                    gender = e.Gender,
                    phoneNumber = e.PhoneNumber,
                    shift = e.Shift,
                    empRole = e.EmpRole,
                    salary = e.Salary,
                    isActive = e.IsActive,
                    discription = e.Discription,
                    empImage = e.EmpImage
                })
                .ToListAsync();

            return Json(data);
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employees employee)
        {
            employee.CreatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                await SaveImage(employee);
                _context.Add(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employees employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await UpdateImage(employee);
                _context.Update(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // ================= DELETE =================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                    return Json(new { success = false, message = "Employee not found!" });

                if (!string.IsNullOrEmpty(employee.EmpImage))
                {
                    var imagePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        employee.EmpImage.TrimStart('/')
                    );

                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error deleting employee!" });
            }
        }

        // ================= IMAGE HANDLING =================
        private async Task SaveImage(Employees employee)
        {
            if (employee.ImageFile != null && employee.ImageFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(employee.ImageFile.FileName).ToLowerInvariant();
                if (new[] { ".jpg", ".jpeg", ".png" }.Contains(fileExtension) &&
                    employee.ImageFile.Length <= 2 * 1024 * 1024)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "employees");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await employee.ImageFile.CopyToAsync(fileStream);
                    }

                    employee.EmpImage = "/images/employees/" + uniqueFileName;
                }
            }
        }

        private async Task UpdateImage(Employees employee)
        {
            if (employee.ImageFile != null && employee.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(employee.EmpImage))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, employee.EmpImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                await SaveImage(employee);
            }
        }
    }
}