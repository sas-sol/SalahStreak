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
    public class WinnerController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public WinnerController(ApplicationDbContext context, ILogger<WinnerController> logger)
            : base(context, logger)
        {
            _dbContext = context;
        }

        // GET: Winner
        public async Task<IActionResult> Index()
        {
            var winners = await _dbContext.Winners
                .Include(w => w.AgeGroup)
                .Include(w => w.Participant)
                .Include(w => w.Round)
                .ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Round", field = "round" },
                new { title = "Participant", field = "participant" },
                new { title = "Age Group", field = "ageGroup" },
                new { title = "Score", field = "score" },
                new { title = "Rank", field = "rank" },
                new { title = "Rewarded", field = "rewarded" },
                new { title = "Created", field = "createdAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = winners.Select(w => new
            {
                id = w.Id,
                round = w.Round?.Name ?? w.RoundId.ToString(),
                participant = w.Participant?.FullName ?? w.ParticipantId.ToString(),
                ageGroup = w.AgeGroup?.Name ?? w.AgeGroupId.ToString(),
                score = w.FinalScore,
                rank = w.RankInAgeGroup,
                rewarded = w.IsRewarded ? "Yes" : "No",
                createdAt = w.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Winner/Edit/{w.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Winner/Details/{w.Id}'>Details</a></li><li><a class='dropdown-item' href='/Winner/Delete/{w.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            ViewData["TableId"] = "winnersTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Winners";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "Winner");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Id", string sortOrder = "desc", string searchTerm = "")
        {
            IQueryable<Winner> query = _dbContext.Winners
                .Include(w => w.AgeGroup)
                .Include(w => w.Participant)
                .Include(w => w.Round);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(w => w.Participant.FullName.Contains(searchTerm) || w.Round.Name.Contains(searchTerm));
            }

            IQueryable<Winner> sortedQuery = sortField.ToLower() switch
            {
                "round" => sortOrder == "asc" ? query.OrderBy(w => w.Round.Name) : query.OrderByDescending(w => w.Round.Name),
                "participant" => sortOrder == "asc" ? query.OrderBy(w => w.Participant.FullName) : query.OrderByDescending(w => w.Participant.FullName),
                "agegroup" => sortOrder == "asc" ? query.OrderBy(w => w.AgeGroup.Name) : query.OrderByDescending(w => w.AgeGroup.Name),
                "score" => sortOrder == "asc" ? query.OrderBy(w => w.FinalScore) : query.OrderByDescending(w => w.FinalScore),
                "rank" => sortOrder == "asc" ? query.OrderBy(w => w.RankInAgeGroup) : query.OrderByDescending(w => w.RankInAgeGroup),
                "createdat" => sortOrder == "asc" ? query.OrderBy(w => w.CreatedAt) : query.OrderByDescending(w => w.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(w => w.Id) : query.OrderByDescending(w => w.Id)
            };

            return await GetTableDataAsync(sortedQuery, w => new
            {
                id = w.Id,
                round = w.Round.Name,
                participant = w.Participant.FullName,
                ageGroup = w.AgeGroup.Name,
                score = w.FinalScore,
                rank = w.RankInAgeGroup,
                rewarded = w.IsRewarded ? "Yes" : "No",
                createdAt = w.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Winner/Edit/{w.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Winner/Details/{w.Id}'>Details</a></li><li><a class='dropdown-item' href='/Winner/Delete/{w.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: Winner/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var winner = await _dbContext.Winners
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
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name");
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email");
            ViewData["RoundId"] = new SelectList(_dbContext.Rounds, "Id", "Name");
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
                _dbContext.Add(winner);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", winner.AgeGroupId);
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email", winner.ParticipantId);
            ViewData["RoundId"] = new SelectList(_dbContext.Rounds, "Id", "Name", winner.RoundId);
            return View(winner);
        }

        // GET: Winner/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var winner = await _dbContext.Winners.FindAsync(id);
            if (winner == null)
            {
                return NotFound();
            }
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", winner.AgeGroupId);
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email", winner.ParticipantId);
            ViewData["RoundId"] = new SelectList(_dbContext.Rounds, "Id", "Name", winner.RoundId);
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
                    _dbContext.Update(winner);
                    await _dbContext.SaveChangesAsync();
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
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", winner.AgeGroupId);
            ViewData["ParticipantId"] = new SelectList(_dbContext.Participants, "Id", "Email", winner.ParticipantId);
            ViewData["RoundId"] = new SelectList(_dbContext.Rounds, "Id", "Name", winner.RoundId);
            return View(winner);
        }

        // GET: Winner/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var winner = await _dbContext.Winners
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
            var winner = await _dbContext.Winners.FindAsync(id);
            if (winner != null)
            {
                _dbContext.Winners.Remove(winner);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WinnerExists(int id)
        {
            return _dbContext.Winners.Any(e => e.Id == id);
        }
    }
}
