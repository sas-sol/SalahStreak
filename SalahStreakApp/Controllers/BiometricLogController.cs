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
    public class BiometricLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BiometricLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BiometricLog
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BiometricLogs.Include(b => b.Participant);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BiometricLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var biometricLog = await _context.BiometricLogs
                .Include(b => b.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (biometricLog == null)
            {
                return NotFound();
            }

            return View(biometricLog);
        }

        // GET: BiometricLog/Create
        public IActionResult Create()
        {
            ViewData["ParticipantId_int"] = new SelectList(_context.Participants, "Id", "Email");
            return View();
        }

        // POST: BiometricLog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParticipantId,ParticipantId_int,CheckInTime,DeviceId,RawData,IsProcessed,CreatedAt")] BiometricLog biometricLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(biometricLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParticipantId_int"] = new SelectList(_context.Participants, "Id", "Email", biometricLog.ParticipantId_int);
            return View(biometricLog);
        }

        // GET: BiometricLog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var biometricLog = await _context.BiometricLogs.FindAsync(id);
            if (biometricLog == null)
            {
                return NotFound();
            }
            ViewData["ParticipantId_int"] = new SelectList(_context.Participants, "Id", "Email", biometricLog.ParticipantId_int);
            return View(biometricLog);
        }

        // POST: BiometricLog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParticipantId,ParticipantId_int,CheckInTime,DeviceId,RawData,IsProcessed,CreatedAt")] BiometricLog biometricLog)
        {
            if (id != biometricLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(biometricLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BiometricLogExists(biometricLog.Id))
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
            ViewData["ParticipantId_int"] = new SelectList(_context.Participants, "Id", "Email", biometricLog.ParticipantId_int);
            return View(biometricLog);
        }

        // GET: BiometricLog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var biometricLog = await _context.BiometricLogs
                .Include(b => b.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (biometricLog == null)
            {
                return NotFound();
            }

            return View(biometricLog);
        }

        // POST: BiometricLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var biometricLog = await _context.BiometricLogs.FindAsync(id);
            if (biometricLog != null)
            {
                _context.BiometricLogs.Remove(biometricLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BiometricLogExists(int id)
        {
            return _context.BiometricLogs.Any(e => e.Id == id);
        }
    }
}
