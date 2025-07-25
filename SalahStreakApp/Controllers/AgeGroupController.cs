using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Controllers
{
    public class AgeGroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgeGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AgeGroup
        public async Task<IActionResult> Index()
        {
            return View(await _context.AgeGroups.ToListAsync());
        }

        // GET: AgeGroup/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ageGroup = await _context.AgeGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ageGroup == null)
            {
                return NotFound();
            }

            return View(ageGroup);
        }

        // GET: AgeGroup/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AgeGroup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,MinAge,MaxAge,Description,IsActive,CreatedAt,UpdatedAt")] AgeGroup ageGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ageGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ageGroup);
        }

        // GET: AgeGroup/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ageGroup = await _context.AgeGroups.FindAsync(id);
            if (ageGroup == null)
            {
                return NotFound();
            }
            return View(ageGroup);
        }

        // POST: AgeGroup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,MinAge,MaxAge,Description,IsActive,CreatedAt,UpdatedAt")] AgeGroup ageGroup)
        {
            if (id != ageGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ageGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgeGroupExists(ageGroup.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ageGroup);
        }

        // GET: AgeGroup/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ageGroup = await _context.AgeGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ageGroup == null)
            {
                return NotFound();
            }

            return View(ageGroup);
        }

        // POST: AgeGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ageGroup = await _context.AgeGroups.FindAsync(id);
            if (ageGroup != null)
            {
                _context.AgeGroups.Remove(ageGroup);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AgeGroupExists(int id)
        {
            return _context.AgeGroups.Any(e => e.Id == id);
        }
    }
}
