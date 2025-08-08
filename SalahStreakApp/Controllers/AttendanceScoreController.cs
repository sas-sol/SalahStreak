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
    public class AttendanceScoreController : BaseController
    {
        public AttendanceScoreController(ApplicationDbContext dbContext, ILogger<AttendanceScoreController> logger)
            : base(dbContext, logger) { }

        // GET: AttendanceScore
        public async Task<IActionResult> Index()
        {
            var scores = await _dbContext.AttendanceScores
                .Include(a => a.AttendanceCalendar)
                .Include(a => a.BiometricLog)
                .Include(a => a.Participant)
                .ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Participant", field = "participant" },
                new { title = "Date", field = "date" },
                new { title = "Score", field = "score" },
                new { title = "Actual", field = "actual" },
                new { title = "Late", field = "isLate" },
                new { title = "Dup", field = "isDuplicate" },
                new { title = "Notes", field = "notes" },
                new { title = "Created", field = "createdAt" },
                new { title = "Updated", field = "updatedAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = scores.Select(s => new
            {
                id = s.Id,
                participant = s.Participant?.FullName ?? s.ParticipantId.ToString(),
                date = s.AttendanceCalendar?.Date.ToString("yyyy-MM-dd") ?? "",
                score = s.Score,
                actual = s.ActualCheckInTime?.ToString("yyyy-MM-dd HH:mm") ?? "",
                isLate = s.IsLate ? "Yes" : "No",
                isDuplicate = s.IsDuplicate ? "Yes" : "No",
                notes = s.Notes ?? "",
                createdAt = s.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = s.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/AttendanceScore/Edit/{s.Id}'>Edit</a></li><li><a class='dropdown-item' href='/AttendanceScore/Details/{s.Id}'>Details</a></li><li><a class='dropdown-item' href='/AttendanceScore/Delete/{s.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            ViewData["TableId"] = "scoresTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Attendance Scores";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "AttendanceScore");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Id", string sortOrder = "desc", string searchTerm = "")
        {
            IQueryable<AttendanceScore> query = _dbContext.AttendanceScores
                .Include(a => a.AttendanceCalendar)
                .Include(a => a.BiometricLog)
                .Include(a => a.Participant);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => (s.Participant.FullName).Contains(searchTerm) || (s.Notes ?? "").Contains(searchTerm));
            }

            IQueryable<AttendanceScore> sortedQuery = sortField.ToLower() switch
            {
                "participant" => sortOrder == "asc" ? query.OrderBy(s => s.Participant.FullName) : query.OrderByDescending(s => s.Participant.FullName),
                "date" => sortOrder == "asc" ? query.OrderBy(s => s.AttendanceCalendar.Date) : query.OrderByDescending(s => s.AttendanceCalendar.Date),
                "score" => sortOrder == "asc" ? query.OrderBy(s => s.Score) : query.OrderByDescending(s => s.Score),
                "createdat" => sortOrder == "asc" ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(s => s.Id) : query.OrderByDescending(s => s.Id)
            };

            return await GetTableDataAsync(sortedQuery, s => new
            {
                id = s.Id,
                participant = s.Participant.FullName,
                date = s.AttendanceCalendar.Date.ToString("yyyy-MM-dd"),
                score = s.Score,
                actual = s.ActualCheckInTime?.ToString("yyyy-MM-dd HH:mm") ?? "",
                isLate = s.IsLate ? "Yes" : "No",
                isDuplicate = s.IsDuplicate ? "Yes" : "No",
                notes = s.Notes ?? "",
                createdAt = s.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = s.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/AttendanceScore/Edit/{s.Id}'>Edit</a></li><li><a class='dropdown-item' href='/AttendanceScore/Details/{s.Id}'>Details</a></li><li><a class='dropdown-item' href='/AttendanceScore/Delete/{s.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: AttendanceScore/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceScore = await _dbContext.AttendanceScores
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
            ViewData["AttendanceCalendarId"] = new SelectList(_dbContext.AttendanceCalendars, "Id", "Id");
            ViewData["BiometricLogId"] = new SelectList(_dbContext.BiometricLogs, "Id", "DeviceId");
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email");
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
                _dbContext.Add(attendanceScore);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AttendanceCalendarId"] = new SelectList(_dbContext.AttendanceCalendars, "Id", "Id", attendanceScore.AttendanceCalendarId);
            ViewData["BiometricLogId"] = new SelectList(_dbContext.BiometricLogs, "Id", "DeviceId", attendanceScore.BiometricLogId);
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email", attendanceScore.ParticipantId);
            return View(attendanceScore);
        }

        // GET: AttendanceScore/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceScore = await _dbContext.AttendanceScores.FindAsync(id);
            if (attendanceScore == null)
            {
                return NotFound();
            }
            ViewData["AttendanceCalendarId"] = new SelectList(_dbContext.AttendanceCalendars, "Id", "Id", attendanceScore.AttendanceCalendarId);
            ViewData["BiometricLogId"] = new SelectList(_dbContext.BiometricLogs, "Id", "DeviceId", attendanceScore.BiometricLogId);
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email", attendanceScore.ParticipantId);
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
                    _dbContext.Update(attendanceScore);
                    await _dbContext.SaveChangesAsync();
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
            ViewData["AttendanceCalendarId"] = new SelectList(_dbContext.AttendanceCalendars, "Id", "Id", attendanceScore.AttendanceCalendarId);
            ViewData["BiometricLogId"] = new SelectList(_dbContext.BiometricLogs, "Id", "DeviceId", attendanceScore.BiometricLogId);
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email", attendanceScore.ParticipantId);
            return View(attendanceScore);
        }

        // GET: AttendanceScore/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceScore = await _dbContext.AttendanceScores
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
            var attendanceScore = await _dbContext.AttendanceScores.FindAsync(id);
            if (attendanceScore != null)
            {
                _dbContext.AttendanceScores.Remove(attendanceScore);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceScoreExists(int id)
        {
            return _dbContext.AttendanceScores.Any(e => e.Id == id);
        }
    }
}
