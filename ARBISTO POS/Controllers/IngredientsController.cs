using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace ARBISTO_POS.Controllers
{
    public class IngredientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IngredientsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }

        // GET: Ingredients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ingredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredients ingredient)
        {
            bool nameExists = await _context.Ingredients
                .AnyAsync(i => i.Name.Trim().ToLower() == ingredient.Name.Trim().ToLower());

            ingredient.CreatedDate = DateTime.UtcNow;

            if (nameExists)
                ModelState.AddModelError(nameof(ingredient.Name), "This ingredient name already exists.");

            if (ModelState.IsValid)
            {
                if (ingredient.ImageFile != null && ingredient.ImageFile.Length > 0)
                {
                    if (ingredient.ImageFile.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "File size cannot exceed 2MB");
                        return View(ingredient);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(ingredient.ImageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Only JPG, JPEG, and PNG files are allowed");
                        return View(ingredient);
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "ingredients");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ingredient.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ingredient.ImageFile.CopyToAsync(fileStream);
                    }

                    ingredient.CateImage = "/images/ingredients/" + uniqueFileName;
                }

                _context.Add(ingredient);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ingredient created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(ingredient);
        }

        // GET: Ingredients/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
                return NotFound();

            return View(ingredient);
        }

        // POST: Ingredients/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ingredients ingredient)
        {
            if (id != ingredient.Id)
                return NotFound();

            bool nameExists = await _context.Ingredients
                .AnyAsync(i =>
                    i.Id != ingredient.Id &&
                    i.Name.Trim().ToLower() == ingredient.Name.Trim().ToLower());

            if (nameExists)
                ModelState.AddModelError(nameof(ingredient.Name), "This ingredient name already exists.");

            if (ModelState.IsValid)
            {
                try
                {
                    if (ingredient.ImageFile != null && ingredient.ImageFile.Length > 0)
                    {
                        if (ingredient.ImageFile.Length > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "File size cannot exceed 2MB");
                            return View(ingredient);
                        }

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var fileExtension = Path.GetExtension(ingredient.ImageFile.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Only JPG, JPEG, and PNG files are allowed");
                            return View(ingredient);
                        }

                        if (!string.IsNullOrEmpty(ingredient.CateImage))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, ingredient.CateImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "ingredients");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ingredient.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ingredient.ImageFile.CopyToAsync(fileStream);
                        }

                        ingredient.CateImage = "/images/ingredients/" + uniqueFileName;
                    }

                    _context.Update(ingredient);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Ingredient updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Ingredients.AnyAsync(e => e.Id == ingredient.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(ingredient);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Ingredients                
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            // 🖼️ Delete image
            if (!string.IsNullOrEmpty(item.CateImage))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    item.CateImage.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }            

            _context.Ingredients.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ingredients deleted successfully!";
            return RedirectToAction(nameof(Index));
        }



        // GET: Ingredients/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ingredient == null)
                return NotFound();

            return View(ingredient);
        }
    }
}
