using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Customers")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CustomersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // =========================
        // ✅ AJAX DATA ENDPOINT
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var data = await _context.Customers
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    email = c.Email,
                    phoneNo = c.PhoneNo,
                    address = c.Address,
                    cusImage = c.CusImage
                })
                .ToListAsync();

            return Json(data);
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var Customers = await _context.Customers.ToListAsync();
            return View(Customers);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customers Customers)
        {
            Customers.CreatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                if (Customers.ImageFile != null && Customers.ImageFile.Length > 0)
                {
                    if (Customers.ImageFile.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "File size cannot exceed 2MB");
                        return View(Customers);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(Customers.ImageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Only JPG, JPEG, and PNG files are allowed");
                        return View(Customers);
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Customers");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Customers.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Customers.ImageFile.CopyToAsync(fileStream);
                    }

                    Customers.CusImage = "/images/Customers/" + uniqueFileName;
                }

                _context.Add(Customers);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Customers created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(Customers);
        }

        // GET: Customers/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var Customers = await _context.Customers.FindAsync(id);
            if (Customers == null)
                return NotFound();

            return View(Customers);
        }

        // POST: Customers/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customers Customers)
        {
            if (id != Customers.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (Customers.ImageFile != null && Customers.ImageFile.Length > 0)
                    {
                        if (Customers.ImageFile.Length > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "File size cannot exceed 2MB");
                            return View(Customers);
                        }

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var fileExtension = Path.GetExtension(Customers.ImageFile.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Only JPG, JPEG, and PNG files are allowed");
                            return View(Customers);
                        }

                        if (!string.IsNullOrEmpty(Customers.CusImage))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, Customers.CusImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Customers");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Customers.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Customers.ImageFile.CopyToAsync(fileStream);
                        }

                        Customers.CusImage = "/images/ingredients/" + uniqueFileName;
                    }

                    _context.Update(Customers);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Customers updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _context.Customers.AnyAsync(e => e.Id == Customers.Id);
                    if (!exists)
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(Customers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Customers.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, message = "Customer not found" });

            if (!string.IsNullOrEmpty(item.CusImage))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    item.CusImage.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Customers.Remove(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Customer deleted successfully" });
        }

        // GET: Customers/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var Customers = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Customers == null)
                return NotFound();

            return View(Customers);
        }
    }
}