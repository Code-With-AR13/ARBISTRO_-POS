using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Pickup Points")]
    public class PickPointsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PickPointsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var points = await _context.PickPoints.ToListAsync();
            return View(points);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PickPoints
                .Select(p => new
                {
                    id = p.Id,
                    picTittle = p.PicTittle,
                    personName = p.PersonName,
                    phoneNo = p.PhoneNo,
                    address = p.Address,
                    picDescription = p.PicDescription,
                    createdDate = p.CreatedDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Json(data);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PickPoints model)
        {
            // Duplicate name check
            if (await _context.PickPoints.AnyAsync(p =>
                p.PicTittle.Trim().ToLower() == model.PicTittle.Trim().ToLower()))
            {
                ModelState.AddModelError("PicTittle", "Pick point already exists.");
            }

            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.UtcNow;
                _context.PickPoints.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pick point created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var point = await _context.PickPoints.FindAsync(id);
            if (point == null) return NotFound();

            return View(point);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PickPoints model)
        {
            if (id != model.Id) return NotFound();

            // Duplicate name check (exclude current)
            if (await _context.PickPoints.AnyAsync(p =>
                p.Id != id &&
                p.PicTittle.Trim().ToLower() == model.PicTittle.Trim().ToLower()))
            {
                ModelState.AddModelError("PicTittle", "Pick point already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pick point updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var point = await _context.PickPoints.FindAsync(id);
                if (point == null)
                    return Json(new { success = false, message = "Pick point not found!" });

                _context.PickPoints.Remove(point);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Pick point deleted successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Error deleting pick point!" });
            }
        }

    }
}
