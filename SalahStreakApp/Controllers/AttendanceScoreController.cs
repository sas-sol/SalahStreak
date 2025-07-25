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
    public class AttendanceScoreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceScoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AttendanceScore
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AttendanceScores.Include(a => a.AttendanceCalendar).Include(a => a.BiometricLog).Include(a => a.Participant);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AttendanceScore/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceScore = await _context.AttendanceScores
                .Include(a => a.AttendanceCalendar)
                .Include(a => a.BiometricLog)
                .Include(a => a.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendanceScore == null)
            {
                return NotFound();
            }

            return View(attendanceScore);
        }

        // GET: AttendanceScore/Create
        public IActionResult Create()
        {
            ViewData["AttendanceCalendarId"] = new SelectList(_context.AttendanceCalendars, "Id", "Id");
            ViewData["BiometricLogId"] = new SelectList(_context.BiometricLogs, "Id", "DeviceId");
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email");
            return View();
        }

        // POST: AttendanceScore/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParticipantId,AttendanceCalendarId,Score,ActualCheckInTime,BiometricLogId,IsLate,IsDuplicate,Notes,CreatedAt,UpdatedAt")] AttendanceScore attendanceScore)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attendanceScore);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AttendanceCalendarId"] = new SelectList(_context.AttendanceCalendars, "Id", "Id", attendanceScore.AttendanceCalendarId);
            ViewData["BiometricLogId"] = new SelectList(_context.BiometricLogs, "Id", "DeviceId", attendanceScore.BiometricLogId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email", attendanceScore.ParticipantId);
            return View(attendanceScore);
        }

        // GET: AttendanceScore/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceScore = await _context.AttendanceScores.FindAsync(id);
            if (attendanceScore == null)
            {
                return NotFound();
            }
            ViewData["AttendanceCalendarId"] = new SelectList(_context.AttendanceCalendars, "Id", "Id", attendanceScore.AttendanceCalendarId);
            ViewData["BiometricLogId"] = new SelectList(_context.BiometricLogs, "Id", "DeviceId", attendanceScore.BiometricLogId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email", attendanceScore.ParticipantId);
            return View(attendanceScore);
        }

        // POST: AttendanceScore/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParticipantId,AttendanceCalendarId,Score,ActualCheckInTime,BiometricLogId,IsLate,IsDuplicate,Notes,CreatedAt,UpdatedAt")] AttendanceScore attendanceScore)
        {
            if (id != attendanceScore.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendanceScore);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceScoreExists(attendanceScore.Id))
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
            ViewData["AttendanceCalendarId"] = new SelectList(_context.AttendanceCalendars, "Id", "Id", attendanceScore.AttendanceCalendarId);
            ViewData["BiometricLogId"] = new SelectList(_context.BiometricLogs, "Id", "DeviceId", attendanceScore.BiometricLogId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Email", attendanceScore.ParticipantId);
            return View(attendanceScore);
        }

        // GET: AttendanceScore/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceScore = await _context.AttendanceScores
                .Include(a => a.AttendanceCalendar)
                .Include(a => a.BiometricLog)
                .Include(a => a.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendanceScore == null)
            {
                return NotFound();
            }

            return View(attendanceScore);
        }

        // POST: AttendanceScore/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendanceScore = await _context.AttendanceScores.FindAsync(id);
            if (attendanceScore != null)
            {
                _context.AttendanceScores.Remove(attendanceScore);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceScoreExists(int id)
        {
            return _context.AttendanceScores.Any(e => e.Id == id);
        }
    }
}
