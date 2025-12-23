using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.IO;

namespace ARBISTO_POS.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ FIXED: Index now properly includes ItemIngredients
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items
                .Include(i => i.FoodCategory)
                .Include(i => i.ItemIngredients)
                    .ThenInclude(ii => ii.Ingredient)
                .ToListAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            return View(new Items());
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.FoodCategory)
                .Include(i => i.ItemIngredients)
                    .ThenInclude(ii => ii.Ingredient)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null) return NotFound();

            ViewBag.ItemIngredients = item.ItemIngredients;

            return View(item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Items item)
        {
            var existingItem = await _context.Items
                .Include(i => i.ItemIngredients)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (existingItem == null) return NotFound();

            // BASIC FIELDS
            existingItem.ItemName = item.ItemName;
            existingItem.ItemDiscription = item.ItemDiscription;
            existingItem.FoodCategoryId = item.FoodCategoryId;

            // IMAGE UPDATE (OPTIONAL)
            if (item.ImageFile != null)
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "images/items");
                Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid() + Path.GetExtension(item.ImageFile.FileName);
                using var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create);
                await item.ImageFile.CopyToAsync(fs);

                existingItem.CateImage = "/images/items/" + fileName;
            }

            // REMOVE OLD INGREDIENTS
            _context.ItemIngredients.RemoveRange(existingItem.ItemIngredients);
            await _context.SaveChangesAsync();

            // ADD NEW INGREDIENTS
            var ingredients = new List<ItemIngredients>();

            foreach (var key in Request.Form.Keys)
            {
                if (!key.EndsWith(".IngredientId")) continue;

                var index = Regex.Match(key, @"\[(\d+)\]").Groups[1].Value;

                ingredients.Add(new ItemIngredients
                {
                    ItemId = id,
                    IngredientId = int.Parse(Request.Form[key]),
                    ConsumptionQty = decimal.Parse(Request.Form[$"ingredients[{index}].ConsumptionQty"]),
                    Cost = decimal.Parse(Request.Form[$"ingredients[{index}].Cost"]),
                    Price = decimal.Parse(Request.Form[$"ingredients[{index}].Price"]),
                    AvailableQty = decimal.Parse(Request.Form[$"ingredients[{index}].AvailableQty"])
                });
            }

            existingItem.ItemCost = (int)Math.Round(ingredients.Sum(x => x.Cost * x.ConsumptionQty));
            existingItem.ItemPrice = (int)Math.Round(ingredients.Sum(x => x.Price * x.ConsumptionQty));

            _context.ItemIngredients.AddRange(ingredients);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item updated successfully!";
            return RedirectToAction(nameof(Index));
        }




        // Keep all your existing methods (Create, GetCategories, GetIngredients)
        [HttpGet]
        public async Task<IActionResult> GetCategories(string search = "")
        {
            var query = _context.FoodCategories.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => (c.CateName ?? "").Contains(search));

            var categories = await query
                .OrderBy(c => c.CateName)
                .Take(50)
                .Select(c => new { id = c.Id.ToString(), text = c.CateName ?? "Unknown" })
                .ToListAsync();

            return Json(new { results = categories });
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
        public async Task<IActionResult> Create(Items item)
        {
            item.CreatedDate = DateTime.UtcNow;
            ModelState.Remove("FoodCategory");
            if (!ModelState.IsValid)
                return View(item);

            if (item.ImageFile != null)
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "images/items");
                Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid() + Path.GetExtension(item.ImageFile.FileName);
                using var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create);
                await item.ImageFile.CopyToAsync(fs);

                item.CateImage = "/images/items/" + fileName;
            }

            var ingredients = new List<ItemIngredients>();

            foreach (var key in Request.Form.Keys)
            {
                if (!key.EndsWith(".IngredientId")) continue;

                var index = Regex.Match(key, @"\[(\d+)\]").Groups[1].Value;

                ingredients.Add(new ItemIngredients
                {
                    IngredientId = int.Parse(Request.Form[key]),
                    ConsumptionQty = decimal.Parse(Request.Form[$"ingredients[{index}].ConsumptionQty"]),
                    Cost = decimal.Parse(Request.Form[$"ingredients[{index}].Cost"]),
                    Price = decimal.Parse(Request.Form[$"ingredients[{index}].Price"]),
                    AvailableQty = decimal.Parse(Request.Form[$"ingredients[{index}].AvailableQty"])
                });
            }

            item.ItemCost = (int)Math.Round(ingredients.Sum(x => x.Cost * x.ConsumptionQty));
            item.ItemPrice = (int)Math.Round(ingredients.Sum(x => x.Price * x.ConsumptionQty));

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            foreach (var ing in ingredients)
            {
                ing.ItemId = item.ItemId;
                _context.ItemIngredients.Add(ing);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Item created successfully!";
            return RedirectToAction(nameof(Index));
        }
        // GET: Items/Delete/5  (optional – agar confirm page nahi chahiye to use na karo)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.ItemIngredients)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items
                .Include(i => i.ItemIngredients)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null) return NotFound();

            // 🖼️ delete image
            if (!string.IsNullOrEmpty(item.CateImage))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    item.CateImage.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            // 🔥 delete child records first
            _context.ItemIngredients.RemoveRange(item.ItemIngredients);
            _context.Items.Remove(item);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


    }
}
