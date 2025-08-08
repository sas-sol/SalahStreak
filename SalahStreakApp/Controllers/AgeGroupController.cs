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
    public class AgeGroupController : BaseController
    {
        public AgeGroupController(ApplicationDbContext dbContext, ILogger<AgeGroupController> logger)
            : base(dbContext, logger) { }

        // GET: AgeGroup
        public async Task<IActionResult> Index()
        {
            var ageGroups = await _dbContext.AgeGroups.ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Name", field = "name" },
                new { title = "Min Age", field = "minAge" },
                new { title = "Max Age", field = "maxAge" },
                new { title = "Description", field = "description" },
                new { title = "Active", field = "isActive" },
                new { title = "Created", field = "createdAt" },
                new { title = "Updated", field = "updatedAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = ageGroups.Select(g => new
            {
                id = g.Id,
                name = g.Name,
                minAge = g.MinAge,
                maxAge = g.MaxAge,
                description = g.Description ?? "",
                isActive = g.IsActive ? "Active" : "Inactive",
                createdAt = g.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = g.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/AgeGroup/Edit/{g.Id}'>Edit</a></li><li><a class='dropdown-item' href='/AgeGroup/Details/{g.Id}'>Details</a></li><li><a class='dropdown-item' href='/AgeGroup/Delete/{g.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            ViewData["TableId"] = "ageGroupsTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Age Groups";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "AgeGroup");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Id", string sortOrder = "desc", string searchTerm = "")
        {
            IQueryable<AgeGroup> query = _dbContext.AgeGroups;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.Name.Contains(searchTerm) || (a.Description ?? "").Contains(searchTerm));
            }

            IQueryable<AgeGroup> sortedQuery = sortField.ToLower() switch
            {
                "name" => sortOrder == "asc" ? query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name),
                "minage" => sortOrder == "asc" ? query.OrderBy(a => a.MinAge) : query.OrderByDescending(a => a.MinAge),
                "maxage" => sortOrder == "asc" ? query.OrderBy(a => a.MaxAge) : query.OrderByDescending(a => a.MaxAge),
                "isactive" => sortOrder == "asc" ? query.OrderBy(a => a.IsActive) : query.OrderByDescending(a => a.IsActive),
                "createdat" => sortOrder == "asc" ? query.OrderBy(a => a.CreatedAt) : query.OrderByDescending(a => a.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(a => a.Id) : query.OrderByDescending(a => a.Id)
            };

            return await GetTableDataAsync(sortedQuery, a => new
            {
                id = a.Id,
                name = a.Name,
                minAge = a.MinAge,
                maxAge = a.MaxAge,
                description = a.Description ?? "",
                isActive = a.IsActive ? "Active" : "Inactive",
                createdAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = a.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/AgeGroup/Edit/{a.Id}'>Edit</a></li><li><a class='dropdown-item' href='/AgeGroup/Details/{a.Id}'>Details</a></li><li><a class='dropdown-item' href='/AgeGroup/Delete/{a.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: AgeGroup/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ageGroup = await _dbContext.AgeGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ageGroup == null)
            {
                return NotFound();
            }

            return View(ageGroup);
        }

        // GET: AgeGroup/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AgeGroup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,MinAge,MaxAge,Description,IsActive,CreatedAt,UpdatedAt")] AgeGroup ageGroup)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(ageGroup);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ageGroup);
        }

        // GET: AgeGroup/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ageGroup = await _dbContext.AgeGroups.FindAsync(id);
            if (ageGroup == null)
            {
                return NotFound();
            }
            return View(ageGroup);
        }

        // POST: AgeGroup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,MinAge,MaxAge,Description,IsActive,CreatedAt,UpdatedAt")] AgeGroup ageGroup)
        {
            if (id != ageGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(ageGroup);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgeGroupExists(ageGroup.Id))
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
            return View(ageGroup);
        }

        // GET: AgeGroup/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ageGroup = await _dbContext.AgeGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ageGroup == null)
            {
                return NotFound();
            }

            return View(ageGroup);
        }

        // POST: AgeGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ageGroup = await _dbContext.AgeGroups.FindAsync(id);
            if (ageGroup != null)
            {
                _dbContext.AgeGroups.Remove(ageGroup);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AgeGroupExists(int id)
        {
            return _dbContext.AgeGroups.Any(e => e.Id == id);
        }
    }
}
