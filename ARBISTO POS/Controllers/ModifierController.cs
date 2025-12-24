using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ARBISTO_POS.Controllers
{
    public class ModifierController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ModifierController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ FIXED: Index now properly includes ModifierIngredients
        public async Task<IActionResult> Index()
        {
            var items = await _context.Modifiers
                .Include(i => i.ModifierIngredients)
                    .ThenInclude(ii => ii.Ingredient)
                .ToListAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            return View(new Modifier());
        }

        public async Task<IActionResult> Edit(int id)
        {
            var modifier = await _context.Modifiers
                .Include(m => m.ModifierIngredients)
                    .ThenInclude(mi => mi.Ingredient)
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (modifier == null)
                return NotFound();

            // ✅ IMPORTANT LINE (THIS WAS MISSING)
            ViewBag.ModifierIngredients = modifier.ModifierIngredients.ToList();

            return View(modifier);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Modifier model)
        {
            var existingModifier = await _context.Modifiers
                .Include(m => m.ModifierIngredients)
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (existingModifier == null)
                return NotFound();

            /* =========================
               BASIC FIELDS
            ========================= */
            existingModifier.ModeName = model.ModeName;
            existingModifier.ModeDiscription = model.ModeDiscription;

            /* =========================
               IMAGE UPDATE
            ========================= */
            if (model.ImageFile != null)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/Modifier");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(fs);

                existingModifier.ModeImage = "/images/Modifier/" + fileName;
            }

            /* =========================
               REMOVE OLD INGREDIENTS
            ========================= */
            _context.ModifierIngredients.RemoveRange(existingModifier.ModifierIngredients);
            await _context.SaveChangesAsync();

            /* =========================
               ADD NEW INGREDIENTS (SAFE)
            ========================= */
            var newIngredients = new List<ModifierIngredients>();

            foreach (var key in Request.Form.Keys)
            {
                if (!key.EndsWith(".IngredientId")) continue;

                var index = Regex.Match(key, @"\[(\d+)\]").Groups[1].Value;
                int ingredientId = int.Parse(Request.Form[key]);

                // 🔒 SAFETY CHECK (FK FIX)
                bool ingredientExists = await _context.Ingredients
                    .AnyAsync(i => i.Id == ingredientId);

                if (!ingredientExists)
                    continue;

                newIngredients.Add(new ModifierIngredients
                {
                    Modifiers = existingModifier,   // ✅ BEST PRACTICE
                    IngredientId = ingredientId,
                    ConsumptionQty = decimal.Parse(Request.Form[$"ingredients[{index}].ConsumptionQty"]),
                    Cost = decimal.Parse(Request.Form[$"ingredients[{index}].Cost"]),
                    Price = decimal.Parse(Request.Form[$"ingredients[{index}].Price"]),
                    AvailableQty = decimal.Parse(Request.Form[$"ingredients[{index}].AvailableQty"])
                });
            }

            existingModifier.ItemCost =
                (int)Math.Round(newIngredients.Sum(x => x.Cost * x.ConsumptionQty));

            existingModifier.ItemPrice =
                (int)Math.Round(newIngredients.Sum(x => x.Price * x.ConsumptionQty));

            _context.ModifierIngredients.AddRange(newIngredients);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Modifier updated successfully!";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> GetIngredients(string search = "")
        {
            var query = _context.Ingredients.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(i => (i.Name ?? "").Contains(search));

            var ingredients = await query
                .OrderBy(i => i.Name)
                .Take(50)
                .Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    cost = i.Cost,
                    price = i.Price,
                    availableQty = i.AvailableQuantity
                })
                .ToListAsync();

            return Json(new { results = ingredients });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Modifier item)
        {
            item.CreatedDate = DateTime.UtcNow;
            ModelState.Remove("FoodCategory");

            if (!ModelState.IsValid)
                return View(item);

            // ================= IMAGE UPLOAD =================
            if (item.ImageFile != null)
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "images/Modifier");
                Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid() + Path.GetExtension(item.ImageFile.FileName);
                using var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create);
                await item.ImageFile.CopyToAsync(fs);

                item.ModeImage = "/images/Modifier/" + fileName;
            }

            // ================= INGREDIENTS =================
            item.ModifierIngredients = new List<ModifierIngredients>();

            foreach (var key in Request.Form.Keys)
            {
                if (!key.EndsWith(".IngredientId")) continue;

                var index = Regex.Match(key, @"\[(\d+)\]").Groups[1].Value;

                item.ModifierIngredients.Add(new ModifierIngredients
                {
                    IngredientId = int.Parse(Request.Form[key]),
                    ConsumptionQty = decimal.Parse(Request.Form[$"ingredients[{index}].ConsumptionQty"]),
                    Cost = decimal.Parse(Request.Form[$"ingredients[{index}].Cost"]),
                    Price = decimal.Parse(Request.Form[$"ingredients[{index}].Price"]),
                    AvailableQty = decimal.Parse(Request.Form[$"ingredients[{index}].AvailableQty"])
                });
            }

            // ================= CALCULATIONS =================
            item.ItemCost = (int)Math.Round(
                item.ModifierIngredients.Sum(x => x.Cost * x.ConsumptionQty));

            item.ItemPrice = (int)Math.Round(
                item.ModifierIngredients.Sum(x => x.Price * x.ConsumptionQty));

            // ================= SAVE ONCE =================
            _context.Modifiers.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Modifier created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var modifier = await _context.Modifiers
                    .Include(m => m.ModifierIngredients)
                    .FirstOrDefaultAsync(m => m.ItemId == id);

                if (modifier == null)
                    return Json(new { success = false, message = "Modifier not found!" });

                // Delete image
                if (!string.IsNullOrEmpty(modifier.ModeImage))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                                 modifier.ModeImage.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                // Delete child records first
                if (modifier.ModifierIngredients != null && modifier.ModifierIngredients.Any())
                    _context.ModifierIngredients.RemoveRange(modifier.ModifierIngredients);

                _context.Modifiers.Remove(modifier);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Modifier deleted successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Unable to delete modifier!" });
            }
        }



    }
}
