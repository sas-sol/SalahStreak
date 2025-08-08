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
    public class RoundController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public RoundController(ApplicationDbContext context, ILogger<RoundController> logger)
            : base(context, logger)
        {
            _dbContext = context;
        }

        // GET: Round
        public async Task<IActionResult> Index()
        {
            var rounds = await _dbContext.Rounds.ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Name", field = "name" },
                new { title = "Start", field = "startDate" },
                new { title = "End", field = "endDate" },
                new { title = "Days", field = "durationDays" },
                new { title = "Active", field = "isActive" },
                new { title = "Completed", field = "isCompleted" },
                new { title = "Created", field = "createdAt" },
                new { title = "Updated", field = "updatedAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = rounds.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                startDate = r.StartDate.ToString("yyyy-MM-dd"),
                endDate = r.EndDate.ToString("yyyy-MM-dd"),
                durationDays = r.DurationDays,
                isActive = r.IsActive ? "Yes" : "No",
                isCompleted = r.IsCompleted ? "Yes" : "No",
                createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = r.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Round/Edit/{r.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Round/Details/{r.Id}'>Details</a></li><li><a class='dropdown-item' href='/Round/Delete/{r.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            ViewData["TableId"] = "roundsTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Rounds";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "Round");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Id", string sortOrder = "desc", string searchTerm = "")
        {
            IQueryable<Round> query = _dbContext.Rounds;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm));
            }

            IQueryable<Round> sortedQuery = sortField.ToLower() switch
            {
                "name" => sortOrder == "asc" ? query.OrderBy(r => r.Name) : query.OrderByDescending(r => r.Name),
                "startdate" => sortOrder == "asc" ? query.OrderBy(r => r.StartDate) : query.OrderByDescending(r => r.StartDate),
                "enddate" => sortOrder == "asc" ? query.OrderBy(r => r.EndDate) : query.OrderByDescending(r => r.EndDate),
                "durationdays" => sortOrder == "asc" ? query.OrderBy(r => r.DurationDays) : query.OrderByDescending(r => r.DurationDays),
                "isactive" => sortOrder == "asc" ? query.OrderBy(r => r.IsActive) : query.OrderByDescending(r => r.IsActive),
                "iscompleted" => sortOrder == "asc" ? query.OrderBy(r => r.IsCompleted) : query.OrderByDescending(r => r.IsCompleted),
                "createdat" => sortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id)
            };

            return await GetTableDataAsync(sortedQuery, r => new
            {
                id = r.Id,
                name = r.Name,
                startDate = r.StartDate.ToString("yyyy-MM-dd"),
                endDate = r.EndDate.ToString("yyyy-MM-dd"),
                durationDays = r.DurationDays,
                isActive = r.IsActive ? "Yes" : "No",
                isCompleted = r.IsCompleted ? "Yes" : "No",
                createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = r.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Round/Edit/{r.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Round/Details/{r.Id}'>Details</a></li><li><a class='dropdown-item' href='/Round/Delete/{r.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: Round/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var round = await _dbContext.Rounds
                .FirstOrDefaultAsync(m => m.Id == id);
            if (round == null)
            {
                return NotFound();
            }

            return View(round);
        }

        // GET: Round/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Round/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,StartDate,EndDate,DurationDays,IsActive,IsCompleted,CreatedAt,UpdatedAt")] Round round)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(round);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(round);
        }

        // GET: Round/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var round = await _dbContext.Rounds.FindAsync(id);
            if (round == null)
            {
                return NotFound();
            }
            return View(round);
        }

        // POST: Round/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,StartDate,EndDate,DurationDays,IsActive,IsCompleted,CreatedAt,UpdatedAt")] Round round)
        {
            if (id != round.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(round);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoundExists(round.Id))
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
            return View(round);
        }

        // GET: Round/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var round = await _dbContext.Rounds
                .FirstOrDefaultAsync(m => m.Id == id);
            if (round == null)
            {
                return NotFound();
            }

            return View(round);
        }

        // POST: Round/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var round = await _dbContext.Rounds.FindAsync(id);
            if (round != null)
            {
                _dbContext.Rounds.Remove(round);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoundExists(int id)
        {
            return _dbContext.Rounds.Any(e => e.Id == id);
        }
    }
}
