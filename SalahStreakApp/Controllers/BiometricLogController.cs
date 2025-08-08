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
    public class BiometricLogController : BaseController
    {
        public BiometricLogController(ApplicationDbContext dbContext, ILogger<BiometricLogController> logger)
            : base(dbContext, logger) { }

        // GET: BiometricLog
        public async Task<IActionResult> Index()
        {
            var logs = await _dbContext.BiometricLogs.Include(b => b.Participant).ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Participant", field = "participant" },
                new { title = "Emp Code", field = "empCode" },
                new { title = "Check-in", field = "checkIn" },
                new { title = "Device", field = "device" },
                new { title = "Processed", field = "processed" },
                new { title = "Created", field = "createdAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = logs.Select(l => new
            {
                id = l.Id,
                participant = l.Participant?.FullName ?? "Unknown",
                empCode = l.ParticipantId,
                checkIn = l.CheckInTime.ToString("yyyy-MM-dd HH:mm"),
                device = l.DeviceId,
                processed = l.IsProcessed ? "Yes" : "No",
                createdAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/BiometricLog/Edit/{l.Id}'>Edit</a></li><li><a class='dropdown-item' href='/BiometricLog/Details/{l.Id}'>Details</a></li><li><a class='dropdown-item' href='/BiometricLog/Delete/{l.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            ViewData["TableId"] = "biometricLogsTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Biometric Logs";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "BiometricLog");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Id", string sortOrder = "desc", string searchTerm = "")
        {
            IQueryable<BiometricLog> query = _dbContext.BiometricLogs.Include(b => b.Participant);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.ParticipantId.Contains(searchTerm) || (l.Participant.FullName).Contains(searchTerm));
            }

            IQueryable<BiometricLog> sortedQuery = sortField.ToLower() switch
            {
                "participant" => sortOrder == "asc" ? query.OrderBy(l => l.Participant.FullName) : query.OrderByDescending(l => l.Participant.FullName),
                "empcode" => sortOrder == "asc" ? query.OrderBy(l => l.ParticipantId) : query.OrderByDescending(l => l.ParticipantId),
                "checkin" => sortOrder == "asc" ? query.OrderBy(l => l.CheckInTime) : query.OrderByDescending(l => l.CheckInTime),
                "device" => sortOrder == "asc" ? query.OrderBy(l => l.DeviceId) : query.OrderByDescending(l => l.DeviceId),
                "processed" => sortOrder == "asc" ? query.OrderBy(l => l.IsProcessed) : query.OrderByDescending(l => l.IsProcessed),
                _ => sortOrder == "asc" ? query.OrderBy(l => l.Id) : query.OrderByDescending(l => l.Id)
            };

            return await GetTableDataAsync(sortedQuery, l => new
            {
                id = l.Id,
                participant = l.Participant.FullName,
                empCode = l.ParticipantId,
                checkIn = l.CheckInTime.ToString("yyyy-MM-dd HH:mm"),
                device = l.DeviceId,
                processed = l.IsProcessed ? "Yes" : "No",
                createdAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/BiometricLog/Edit/{l.Id}'>Edit</a></li><li><a class='dropdown-item' href='/BiometricLog/Details/{l.Id}'>Details</a></li><li><a class='dropdown-item' href='/BiometricLog/Delete/{l.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: BiometricLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var biometricLog = await _dbContext.BiometricLogs
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
            ViewData["ParticipantId_int"] = new SelectList(_dbContext.Participants, "Id", "Email");
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
                _dbContext.Add(biometricLog);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParticipantId_int"] = new SelectList(_dbContext.Participants, "Id", "Email", biometricLog.ParticipantId_int);
            return View(biometricLog);
        }

        // GET: BiometricLog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var biometricLog = await _dbContext.BiometricLogs.FindAsync(id);
            if (biometricLog == null)
            {
                return NotFound();
            }
            ViewData["ParticipantId_int"] = new SelectList(_dbContext.Participants, "Id", "Email", biometricLog.ParticipantId_int);
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
                    _dbContext.Update(biometricLog);
                    await _dbContext.SaveChangesAsync();
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
            ViewData["ParticipantId_int"] = new SelectList(_dbContext.Participants, "Id", "Email", biometricLog.ParticipantId_int);
            return View(biometricLog);
        }

        // GET: BiometricLog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var biometricLog = await _dbContext.BiometricLogs
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
            var biometricLog = await _dbContext.BiometricLogs.FindAsync(id);
            if (biometricLog != null)
            {
                _dbContext.BiometricLogs.Remove(biometricLog);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BiometricLogExists(int id)
        {
            return _dbContext.BiometricLogs.Any(e => e.Id == id);
        }
    }
}
