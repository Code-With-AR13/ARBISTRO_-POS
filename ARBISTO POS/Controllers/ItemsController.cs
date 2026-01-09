using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.RegularExpressions;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Items")]
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

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

        // ✅ UPDATED: Create with Toastr
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Items item)
        {
            item.CreatedDate = DateTime.UtcNow;
            ModelState.Remove("FoodCategory");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly!";
                return View(item);
            }

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

                int ingredientId = int.Parse(Request.Form[key]);
                decimal consumptionQty = decimal.Parse(Request.Form[$"ingredients[{index}].ConsumptionQty"]);

                var ingredient = await _context.Ingredients.FindAsync(ingredientId);

                if (ingredient == null)
                {
                    TempData["ErrorMessage"] = $"Ingredient with ID {ingredientId} not found!";
                    return View(item);
                }

                // ✅ Toastr Error for Insufficient Quantity
                if (ingredient.AvailableQuantity < consumptionQty)
                {
                    TempData["ErrorMessage"] = $"Insufficient quantity for {ingredient.Name}! Available: {ingredient.AvailableQuantity}, Required: {consumptionQty}";
                    return View(item);
                }

                ingredient.AvailableQuantity -= consumptionQty;

                ingredients.Add(new ItemIngredients
                {
                    IngredientId = ingredientId,
                    ConsumptionQty = consumptionQty,
                    Cost = decimal.Parse(Request.Form[$"ingredients[{index}].Cost"]),
                    Price = decimal.Parse(Request.Form[$"ingredients[{index}].Price"]),
                    AvailableQty = ingredient.AvailableQuantity
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
            TempData["SuccessMessage"] = "Item created successfully and ingredient quantities updated!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ UPDATED: Edit with Toastr
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Items item)
        {
            var existingItem = await _context.Items
                .Include(i => i.ItemIngredients)
                    .ThenInclude(ii => ii.Ingredient)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (existingItem == null)
            {
                TempData["ErrorMessage"] = "Item not found!";
                return NotFound();
            }

            // Restore old quantities
            foreach (var oldIng in existingItem.ItemIngredients)
            {
                var ingredient = await _context.Ingredients.FindAsync(oldIng.IngredientId);
                if (ingredient != null)
                {
                    ingredient.AvailableQuantity += oldIng.ConsumptionQty;
                }
            }

            existingItem.ItemName = item.ItemName;
            existingItem.ItemDiscription = item.ItemDiscription;
            existingItem.FoodCategoryId = item.FoodCategoryId;

            if (item.ImageFile != null)
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "images/items");
                Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid() + Path.GetExtension(item.ImageFile.FileName);
                using var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create);
                await item.ImageFile.CopyToAsync(fs);

                existingItem.CateImage = "/images/items/" + fileName;
            }

            _context.ItemIngredients.RemoveRange(existingItem.ItemIngredients);
            await _context.SaveChangesAsync();

            var ingredients = new List<ItemIngredients>();

            foreach (var key in Request.Form.Keys)
            {
                if (!key.EndsWith(".IngredientId")) continue;

                var index = Regex.Match(key, @"\[(\d+)\]").Groups[1].Value;

                int ingredientId = int.Parse(Request.Form[key]);
                decimal consumptionQty = decimal.Parse(Request.Form[$"ingredients[{index}].ConsumptionQty"]);

                var ingredient = await _context.Ingredients.FindAsync(ingredientId);

                if (ingredient == null)
                {
                    TempData["ErrorMessage"] = $"Ingredient not found!";
                    return View(item);
                }

                // ✅ Toastr Error for Insufficient Quantity
                if (ingredient.AvailableQuantity < consumptionQty)
                {
                    TempData["ErrorMessage"] = $"Insufficient quantity for {ingredient.Name}! Available: {ingredient.AvailableQuantity}, Required: {consumptionQty}";
                    return View(item);
                }

                ingredient.AvailableQuantity -= consumptionQty;

                ingredients.Add(new ItemIngredients
                {
                    ItemId = id,
                    IngredientId = ingredientId,
                    ConsumptionQty = consumptionQty,
                    Cost = decimal.Parse(Request.Form[$"ingredients[{index}].Cost"]),
                    Price = decimal.Parse(Request.Form[$"ingredients[{index}].Price"]),
                    AvailableQty = ingredient.AvailableQuantity
                });
            }

            existingItem.ItemCost = (int)Math.Round(ingredients.Sum(x => x.Cost * x.ConsumptionQty));
            existingItem.ItemPrice = (int)Math.Round(ingredients.Sum(x => x.Price * x.ConsumptionQty));

            _context.ItemIngredients.AddRange(ingredients);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item updated and ingredient quantities adjusted!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.Items
                    .Include(i => i.ItemIngredients)
                    .FirstOrDefaultAsync(i => i.ItemId == id);

                if (item == null)
                    return Json(new { success = false, message = "Item not found" });

                foreach (var itemIng in item.ItemIngredients)
                {
                    var ingredient = await _context.Ingredients.FindAsync(itemIng.IngredientId);
                    if (ingredient != null)
                    {
                        ingredient.AvailableQuantity += itemIng.ConsumptionQty;
                    }
                }

                if (!string.IsNullOrEmpty(item.CateImage))
                {
                    string imagePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        item.CateImage.TrimStart('/')
                    );

                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                if (item.ItemIngredients != null && item.ItemIngredients.Any())
                    _context.ItemIngredients.RemoveRange(item.ItemIngredients);

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Item deleted and ingredient quantities restored!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Unable to delete item: " + ex.Message });
            }
        }

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
    }
}
