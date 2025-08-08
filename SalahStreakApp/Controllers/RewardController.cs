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
    public class RewardController : BaseController
    {
        public RewardController(ApplicationDbContext dbContext, ILogger<RewardController> logger)
            : base(dbContext, logger) { }

        // GET: Reward
        public async Task<IActionResult> Index()
        {
            var rewards = await _dbContext.Rewards.Include(r => r.AgeGroup).ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Title", field = "title" },
                new { title = "Age Group", field = "ageGroup" },
                new { title = "Qty", field = "qty" },
                new { title = "Status", field = "status" },
                new { title = "Delivered At", field = "deliveredAt" },
                new { title = "Created", field = "createdAt" },
                new { title = "Updated", field = "updatedAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = rewards.Select(r => new
            {
                id = r.Id,
                title = r.Title,
                ageGroup = r.AgeGroup?.Name ?? "",
                qty = r.Quantity,
                status = r.DeliveryStatus,
                deliveredAt = r.DeliveredAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = r.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Reward/Edit/{r.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Reward/Details/{r.Id}'>Details</a></li><li><a class='dropdown-item' href='/Reward/Delete/{r.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            ViewData["TableId"] = "rewardsTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Rewards";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "Reward");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Id", string sortOrder = "desc", string searchTerm = "")
        {
            IQueryable<Reward> query = _dbContext.Rewards.Include(r => r.AgeGroup);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.Title.Contains(searchTerm) || (r.Description ?? "").Contains(searchTerm));
            }

            IQueryable<Reward> sortedQuery = sortField.ToLower() switch
            {
                "title" => sortOrder == "asc" ? query.OrderBy(r => r.Title) : query.OrderByDescending(r => r.Title),
                "qty" => sortOrder == "asc" ? query.OrderBy(r => r.Quantity) : query.OrderByDescending(r => r.Quantity),
                "status" => sortOrder == "asc" ? query.OrderBy(r => r.DeliveryStatus) : query.OrderByDescending(r => r.DeliveryStatus),
                "createdat" => sortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id)
            };

            return await GetTableDataAsync(sortedQuery, r => new
            {
                id = r.Id,
                title = r.Title,
                ageGroup = r.AgeGroup.Name,
                qty = r.Quantity,
                status = r.DeliveryStatus,
                deliveredAt = r.DeliveredAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = r.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Reward/Edit/{r.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Reward/Details/{r.Id}'>Details</a></li><li><a class='dropdown-item' href='/Reward/Delete/{r.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: Reward/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reward = await _dbContext.Rewards
                .Include(r => r.AgeGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reward == null)
            {
                return NotFound();
            }

            return View(reward);
        }

        // GET: Reward/Create
        public IActionResult Create()
        {
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name");
            return View();
        }

        // POST: Reward/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ImageUrl,Quantity,AgeGroupId,DeliveryStatus,DeliveredAt,IsActive,CreatedAt,UpdatedAt")] Reward reward)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(reward);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", reward.AgeGroupId);
            return View(reward);
        }

        // GET: Reward/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reward = await _dbContext.Rewards.FindAsync(id);
            if (reward == null)
            {
                return NotFound();
            }
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", reward.AgeGroupId);
            return View(reward);
        }

        // POST: Reward/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ImageUrl,Quantity,AgeGroupId,DeliveryStatus,DeliveredAt,IsActive,CreatedAt,UpdatedAt")] Reward reward)
        {
            if (id != reward.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(reward);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RewardExists(reward.Id))
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
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", reward.AgeGroupId);
            return View(reward);
        }

        // GET: Reward/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reward = await _dbContext.Rewards
                .Include(r => r.AgeGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reward == null)
            {
                return NotFound();
            }

            return View(reward);
        }

        // POST: Reward/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reward = await _dbContext.Rewards.FindAsync(id);
            if (reward != null)
            {
                _dbContext.Rewards.Remove(reward);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RewardExists(int id)
        {
            return _dbContext.Rewards.Any(e => e.Id == id);
        }
    }
}
