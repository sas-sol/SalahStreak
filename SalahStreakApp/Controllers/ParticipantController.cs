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
    public class ParticipantController : BaseController
    {
        public ParticipantController(ApplicationDbContext dbContext, ILogger<ParticipantController> logger) 
            : base(dbContext, logger)
        {
        }

        public async Task<IActionResult> Index()
        {
            var participants = await _dbContext.Participants
                .Include(p => p.AgeGroup)
                .ToListAsync();

            var columns = new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Name", field = "fullName" },
                new { title = "Age", field = "age" },
                new { title = "Gender", field = "gender" },
                new { title = "Phone", field = "phone" },
                new { title = "Email", field = "email" },
                new { title = "Participant ID", field = "participantId" },
                new { title = "Age Group", field = "ageGroupName" },
                new { title = "Status", field = "isActive" },
                new { title = "Created", field = "createdAt" },
                new { title = "Actions", field = "actions" }
            };

            var data = participants.Select(p => new
            {
                id = p.Id,
                fullName = p.FullName,
                age = p.Age,
                gender = p.Gender,
                phone = p.Phone,
                email = p.Email,
                participantId = p.ParticipantId,
                ageGroupName = p.AgeGroup?.Name ?? "N/A",
                isActive = p.IsActive ? "Active" : "Inactive",
                createdAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = p.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Participant/Edit/{p.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Participant/Details/{p.Id}'>Details</a></li><li><a class='dropdown-item' href='/Participant/Delete/{p.Id}'>Delete</a></li></ul></div>"
            }).ToList();

            // Set ViewData for the _DataTable partial
            ViewData["TableId"] = "participantsTable";
            ViewData["Columns"] = columns;
            ViewData["TableData"] = data;
            ViewData["TableTitle"] = "Participants";
            ViewData["ShowExport"] = true;
            ViewData["ShowSearch"] = true;
            ViewData["ShowRefresh"] = true;
            ViewData["RefreshUrl"] = Url.Action("GetData", "Participant");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int page = 1, int size = 25, string sortField = "Id", string sortOrder = "desc", string searchTerm = "")
        {
            // Start with base query
            IQueryable<Participant> query = _dbContext.Participants.Include(p => p.AgeGroup);

            // Apply search if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => 
                    p.FullName.Contains(searchTerm) || 
                    p.Email.Contains(searchTerm) || 
                    p.ParticipantId.Contains(searchTerm) ||
                    p.Phone.Contains(searchTerm));
            }

            // Apply sorting
            IQueryable<Participant> sortedQuery = sortField.ToLower() switch
            {
                "fullname" => sortOrder == "asc" ? query.OrderBy(p => p.FullName) : query.OrderByDescending(p => p.FullName),
                "age" => sortOrder == "asc" ? query.OrderBy(p => p.Age) : query.OrderByDescending(p => p.Age),
                "gender" => sortOrder == "asc" ? query.OrderBy(p => p.Gender) : query.OrderByDescending(p => p.Gender),
                "email" => sortOrder == "asc" ? query.OrderBy(p => p.Email) : query.OrderByDescending(p => p.Email),
                "participantid" => sortOrder == "asc" ? query.OrderBy(p => p.ParticipantId) : query.OrderByDescending(p => p.ParticipantId),
                "isactive" => sortOrder == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                "createdat" => sortOrder == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id)
            };

            return await GetTableDataAsync(sortedQuery, p => new
            {
                id = p.Id,
                fullName = p.FullName,
                age = p.Age,
                gender = p.Gender,
                phone = p.Phone,
                email = p.Email,
                participantId = p.ParticipantId,
                ageGroupName = p.AgeGroup?.Name ?? "N/A",
                isActive = p.IsActive ? "Active" : "Inactive",
                createdAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                updatedAt = p.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                actions = $"<div class='btn-group'><button class='btn btn-sm btn-outline-primary dropdown-toggle' data-bs-toggle='dropdown'>Actions</button><ul class='dropdown-menu'><li><a class='dropdown-item' href='/Participant/Edit/{p.Id}'>Edit</a></li><li><a class='dropdown-item' href='/Participant/Details/{p.Id}'>Details</a></li><li><a class='dropdown-item' href='/Participant/Delete/{p.Id}'>Delete</a></li></ul></div>"
            }, page, size, sortField, sortOrder, searchTerm);
        }

        // GET: Participant/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _dbContext.Participants
                .Include(p => p.AgeGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participant == null)
            {
                return NotFound();
            }

            return View(participant);
        }

        // GET: Participant/Create
        public IActionResult Create()
        {
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name");
            return View();
        }

        // POST: Participant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Age,Gender,Phone,Email,ParticipantId,ParentName,ParentCNIC,AgeGroupId,IsActive,CreatedAt,UpdatedAt")] Participant participant)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(participant);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", participant.AgeGroupId);
            return View(participant);
        }

        // GET: Participant/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _dbContext.Participants.FindAsync(id);
            if (participant == null)
            {
                return NotFound();
            }
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", participant.AgeGroupId);
            return View(participant);
        }

        // POST: Participant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Age,Gender,Phone,Email,ParticipantId,ParentName,ParentCNIC,AgeGroupId,IsActive,CreatedAt,UpdatedAt")] Participant participant)
        {
            if (id != participant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(participant);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParticipantExists(participant.Id))
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
            ViewData["AgeGroupId"] = new SelectList(_dbContext.AgeGroups, "Id", "Name", participant.AgeGroupId);
            return View(participant);
        }

        // GET: Participant/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _dbContext.Participants
                .Include(p => p.AgeGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participant == null)
            {
                return NotFound();
            }

            return View(participant);
        }

        // POST: Participant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participant = await _dbContext.Participants.FindAsync(id);
            if (participant != null)
            {
                _dbContext.Participants.Remove(participant);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParticipantExists(int id)
        {
            return _dbContext.Participants.Any(e => e.Id == id);
        }
    }
}
