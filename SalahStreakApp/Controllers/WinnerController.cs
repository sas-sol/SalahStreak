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
    public class WinnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WinnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Winner
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Winners.Include(w => w.AgeGroup).Include(w => w.Participant).Include(w => w.Round);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Winner/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var winner = await _context.Winners
                .Include(w => w.AgeGroup)
                .Include(w => w.Participant)
                .Include(w => w.Round)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (winner == null)
            {
                return NotFound();
            }

            return View(winner);
        }

        // GET: Winner/Create
        public IActionResult Create()
        {
            ViewData["AgeGroupId"] = new SelectList(_context.AgeGroups, "Id", "Name");
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email");
            ViewData["RoundId"] = new SelectList(_context.Rounds, "Id", "Name");
            return View();
        }

        // POST: Winner/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoundId,ParticipantId,AgeGroupId,FinalScore,RankInAgeGroup,IsRewarded,RewardedAt,CreatedAt")] Winner winner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(winner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgeGroupId"] = new SelectList(_context.AgeGroups, "Id", "Name", winner.AgeGroupId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email", winner.ParticipantId);
            ViewData["RoundId"] = new SelectList(_context.Rounds, "Id", "Name", winner.RoundId);
            return View(winner);
        }

        // GET: Winner/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var winner = await _context.Winners.FindAsync(id);
            if (winner == null)
            {
                return NotFound();
            }
            ViewData["AgeGroupId"] = new SelectList(_context.AgeGroups, "Id", "Name", winner.AgeGroupId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email", winner.ParticipantId);
            ViewData["RoundId"] = new SelectList(_context.Rounds, "Id", "Name", winner.RoundId);
            return View(winner);
        }

        // POST: Winner/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoundId,ParticipantId,AgeGroupId,FinalScore,RankInAgeGroup,IsRewarded,RewardedAt,CreatedAt")] Winner winner)
        {
            if (id != winner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(winner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WinnerExists(winner.Id))
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
            ViewData["AgeGroupId"] = new SelectList(_context.AgeGroups, "Id", "Name", winner.AgeGroupId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email", winner.ParticipantId);
            ViewData["RoundId"] = new SelectList(_context.Rounds, "Id", "Name", winner.RoundId);
            return View(winner);
        }

        // GET: Winner/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var winner = await _context.Winners
                .Include(w => w.AgeGroup)
                .Include(w => w.Participant)
                .Include(w => w.Round)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (winner == null)
            {
                return NotFound();
            }

            return View(winner);
        }

        // POST: Winner/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var winner = await _context.Winners.FindAsync(id);
            if (winner != null)
            {
                _context.Winners.Remove(winner);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WinnerExists(int id)
        {
            return _context.Winners.Any(e => e.Id == id);
        }
    }
}
