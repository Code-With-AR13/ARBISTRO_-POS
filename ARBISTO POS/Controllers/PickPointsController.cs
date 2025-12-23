using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
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

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var point = await _context.PickPoints.FindAsync(id);
                if (point == null) return NotFound();

                _context.PickPoints.Remove(point);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pick point deleted successfully!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Error deleting pick point!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
