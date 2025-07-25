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
    public class AttendanceCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceCalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AttendanceCalendar
        public async Task<IActionResult> Index()
        {
            return View(await _context.AttendanceCalendars.ToListAsync());
        }

        // GET: AttendanceCalendar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceCalendar = await _context.AttendanceCalendars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendanceCalendar == null)
            {
                return NotFound();
            }

            return View(attendanceCalendar);
        }

        // GET: AttendanceCalendar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AttendanceCalendar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,ExpectedTime,TimeWindowMinutes,Description,IsActive,CreatedAt,UpdatedAt")] AttendanceCalendar attendanceCalendar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attendanceCalendar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attendanceCalendar);
        }

        // GET: AttendanceCalendar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceCalendar = await _context.AttendanceCalendars.FindAsync(id);
            if (attendanceCalendar == null)
            {
                return NotFound();
            }
            return View(attendanceCalendar);
        }

        // POST: AttendanceCalendar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,ExpectedTime,TimeWindowMinutes,Description,IsActive,CreatedAt,UpdatedAt")] AttendanceCalendar attendanceCalendar)
        {
            if (id != attendanceCalendar.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendanceCalendar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceCalendarExists(attendanceCalendar.Id))
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
            return View(attendanceCalendar);
        }

        // GET: AttendanceCalendar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceCalendar = await _context.AttendanceCalendars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendanceCalendar == null)
            {
                return NotFound();
            }

            return View(attendanceCalendar);
        }

        // POST: AttendanceCalendar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendanceCalendar = await _context.AttendanceCalendars.FindAsync(id);
            if (attendanceCalendar != null)
            {
                _context.AttendanceCalendars.Remove(attendanceCalendar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceCalendarExists(int id)
        {
            return _context.AttendanceCalendars.Any(e => e.Id == id);
        }
    }
}
