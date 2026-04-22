using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Service Tables")]
    public class ServiceTablesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServiceTablesController(ApplicationDbContext context,
                                       IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var tables = await _context.ServiceTables.ToListAsync();
            return View(tables);
        }

        // ================= AJAX GET ALL =================
        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _context.ServiceTables
                .Select(x => new
                {
                    id = x.Id,
                    tabName = x.TabName,
                    tabDescription = x.TabDescription,
                    tabImage = x.TabImage,
                    createdDate = x.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                })
                .ToList();

            return Json(new { data });
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTables table)
        {
            // ✅ SET CURRENT DATETIME HERE
            table.CreatedDate = DateTime.UtcNow;

            if (await _context.ServiceTables.AnyAsync(t =>
                t.TabName.Trim().ToLower() == table.TabName.Trim().ToLower()))
            {
                ModelState.AddModelError("TabName", "Table name already exists.");
            }

            if (ModelState.IsValid)
            {
                await SaveImage(table);
                _context.ServiceTables.Add(table);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service table created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(table);
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var table = await _context.ServiceTables.FindAsync(id);
            if (table == null) return NotFound();

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceTables table)
        {
            if (id != table.Id) return NotFound();

            if (await _context.ServiceTables.AnyAsync(t =>
                t.Id != id &&
                t.TabName.Trim().ToLower() == table.TabName.Trim().ToLower()))
            {
                ModelState.AddModelError("TabName", "Table name already exists.");
            }

            if (ModelState.IsValid)
            {
                await UpdateImage(table);
                _context.Update(table);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service table updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAjax(int id)
        {
            try
            {
                var table = await _context.ServiceTables.FindAsync(id);
                if (table == null)
                    return Json(new { success = false, message = "Table not found!" });

                // Delete image
                if (!string.IsNullOrEmpty(table.TabImage))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, table.TabImage.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.ServiceTables.Remove(table);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Service table deleted successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Error deleting service table!" });
            }
        }

        // ================= IMAGE SAVE =================
        private async Task SaveImage(ServiceTables table)
        {
            if (table.ImageFile != null && table.ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(table.ImageFile.FileName).ToLowerInvariant();

                if (new[] { ".jpg", ".jpeg", ".png" }.Contains(ext) &&
                    table.ImageFile.Length <= 2 * 1024 * 1024)
                {
                    var folder = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "images",
                        "tables");

                    Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + ext;
                    var filePath = Path.Combine(folder, fileName);

                    using var fs = new FileStream(filePath, FileMode.Create);
                    await table.ImageFile.CopyToAsync(fs);

                    table.TabImage = "/images/tables/" + fileName;
                }
            }
        }

        // ================= IMAGE UPDATE =================
        private async Task UpdateImage(ServiceTables table)
        {
            if (table.ImageFile != null && table.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(table.TabImage))
                {
                    var oldPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        table.TabImage.TrimStart('/'));

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                await SaveImage(table);
            }
        }
    }
}