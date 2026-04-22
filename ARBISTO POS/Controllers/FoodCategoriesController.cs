using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Category")]
    public class FoodCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FoodCategoriesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.FoodCategories.ToListAsync();
            return View(categories);
        }

        // ✅ ✅ AJAX METHOD (ONLY ADDED - NOTHING MODIFIED)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.FoodCategories
                .Select(x => new
                {
                    id = x.Id,
                    cateName = x.CateName,
                    description = x.Description,
                    cateImage = x.CateImage
                })
                .ToListAsync();

            return Json(new { data });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodCategories category)
        {
            category.CreatedDate = DateTime.UtcNow;

            if (await _context.FoodCategories.AnyAsync(c =>
                c.CateName.Trim().ToLower() == category.CateName.Trim().ToLower()))
            {
                ModelState.AddModelError("CateName", "Category name already exists.");
            }

            if (ModelState.IsValid)
            {
                await SaveImage(category);
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.FoodCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodCategories category)
        {
            if (id != category.Id) return NotFound();

            if (await _context.FoodCategories.AnyAsync(c =>
                c.Id != id && c.CateName.Trim().ToLower() == category.CateName.Trim().ToLower()))
            {
                ModelState.AddModelError("CateName", "Category name already exists.");
            }

            if (ModelState.IsValid)
            {
                await UpdateImage(category);
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.FoodCategories.FindAsync(id);
                if (category == null)
                    return Json(new { success = false, message = "Category not found" });

                bool isUsed = await _context.Items.AnyAsync(f => f.FoodCategoryId == id);
                if (isUsed)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This category cannot be deleted because it is used in some Items."
                    });
                }

                if (!string.IsNullOrEmpty(category.CateImage))
                {
                    var imagePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        category.CateImage.TrimStart('/')
                    );

                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.FoodCategories.Remove(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Category deleted successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Error deleting category!" });
            }
        }

        private async Task SaveImage(FoodCategories category)
        {
            if (category.ImageFile != null && category.ImageFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(category.ImageFile.FileName).ToLowerInvariant();

                if (new[] { ".jpg", ".jpeg", ".png" }.Contains(fileExtension) &&
                    category.ImageFile.Length <= 2 * 1024 * 1024)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await category.ImageFile.CopyToAsync(fileStream);
                    }

                    category.CateImage = "/images/categories/" + uniqueFileName;
                }
            }
        }

        private async Task UpdateImage(FoodCategories category)
        {
            if (category.ImageFile != null && category.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(category.CateImage))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, category.CateImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                await SaveImage(category);
            }
        }
    }
}