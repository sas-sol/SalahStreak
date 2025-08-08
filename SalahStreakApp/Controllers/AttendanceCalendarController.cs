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
    public class AttendanceCalendarController : BaseController
    {
        public AttendanceCalendarController(ApplicationDbContext dbContext, ILogger<AttendanceCalendarController> logger)
            : base(dbContext, logger) { }

        // GET: AttendanceCalendar
        public async Task<IActionResult> Index()
        {
            var calendars = await _dbContext.AttendanceCalendars.ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Date", field = "date" },
                new { title = "Expected", field = "expectedTime" },
                new { title = "Window (min)", field = "timeWindow" },
                new { title = "Description", field = "description" },
                new { title = "Active", field = "isActive" },
                new { title = "Created", field = "createdAt" },
                new { title = "Updated", field = "updatedAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = calendars.Select(c => new
            {
                id = c.Id,
                date = c.Date.ToString("yyyy-MM-dd"),
                expectedTime = c.ExpectedTime.ToString(),
                timeWindow = c.TimeWindowMinutes,
                description = c.Description ?? "",
                isActive = c.IsActive ? "Active" : "Inactive",
                createdAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = c.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/AttendanceCalendar/Edit/{c.Id}'>Edit</a></li><li><a class='dropdown-item' href='/AttendanceCalendar/Details/{c.Id}'>Details</a></li><li><a class='dropdown-item' href='/AttendanceCalendar/Delete/{c.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            ViewData["TableId"] = "calendarsTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Attendance Calendar";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "AttendanceCalendar");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Date", string sortOrder = "desc", string searchTerm = "")
        {
            IQueryable<AttendanceCalendar> query = _dbContext.AttendanceCalendars;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => (c.Description ?? "").Contains(searchTerm));
            }

            IQueryable<AttendanceCalendar> sortedQuery = sortField.ToLower() switch
            {
                "date" => sortOrder == "asc" ? query.OrderBy(c => c.Date) : query.OrderByDescending(c => c.Date),
                "expected" => sortOrder == "asc" ? query.OrderBy(c => c.ExpectedTime) : query.OrderByDescending(c => c.ExpectedTime),
                "timewindow" => sortOrder == "asc" ? query.OrderBy(c => c.TimeWindowMinutes) : query.OrderByDescending(c => c.TimeWindowMinutes),
                "isactive" => sortOrder == "asc" ? query.OrderBy(c => c.IsActive) : query.OrderByDescending(c => c.IsActive),
                "createdat" => sortOrder == "asc" ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id)
            };

            return await GetTableDataAsync(sortedQuery, c => new
            {
                id = c.Id,
                date = c.Date.ToString("yyyy-MM-dd"),
                expectedTime = c.ExpectedTime.ToString(),
                timeWindow = c.TimeWindowMinutes,
                description = c.Description ?? "",
                isActive = c.IsActive ? "Active" : "Inactive",
                createdAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = c.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/AttendanceCalendar/Edit/{c.Id}'>Edit</a></li><li><a class='dropdown-item' href='/AttendanceCalendar/Details/{c.Id}'>Details</a></li><li><a class='dropdown-item' href='/AttendanceCalendar/Delete/{c.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: AttendanceCalendar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceCalendar = await _dbContext.AttendanceCalendars
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
                _dbContext.Add(attendanceCalendar);
                await _dbContext.SaveChangesAsync();
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

            var attendanceCalendar = await _dbContext.AttendanceCalendars.FindAsync(id);
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
                    _dbContext.Update(attendanceCalendar);
                    await _dbContext.SaveChangesAsync();
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

            var attendanceCalendar = await _dbContext.AttendanceCalendars
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
            var attendanceCalendar = await _dbContext.AttendanceCalendars.FindAsync(id);
            if (attendanceCalendar != null)
            {
                _dbContext.AttendanceCalendars.Remove(attendanceCalendar);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceCalendarExists(int id)
        {
            return _dbContext.AttendanceCalendars.Any(e => e.Id == id);
        }
    }
}
