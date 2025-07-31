using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly ILogger _logger;

        protected BaseController(ApplicationDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        protected async Task<IActionResult> GetTableDataAsync<T>(
            IQueryable<T> query, 
            Func<T, object> dataSelector,
            int page = 1, 
            int size = 25,
            string sortField = "Id",
            string sortOrder = "desc",
            string searchTerm = "") where T : class
        {
            try
            {
                // Apply search if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = ApplySearch(query, searchTerm);
                }

                // Apply sorting
                query = ApplySorting(query, sortField, sortOrder);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                // Transform data
                var data = items.Select(dataSelector).ToList();

                return Json(new
                {
                    data = data,
                    total = totalCount,
                    page = page,
                    size = size,
                    pages = (int)Math.Ceiling((double)totalCount / size)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table data");
                return Json(new { error = "Error loading data" });
            }
        }

        // These methods should be overridden in derived controllers
        protected virtual IQueryable<T> ApplySearch<T>(IQueryable<T> query, string searchTerm) where T : class
        {
            // Default implementation - override in derived controllers
            return query;
        }

        protected virtual IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortField, string sortOrder) where T : class
        {
            // Default implementation - override in derived controllers
            return query;
        }

        protected object[] GetCommonColumns()
        {
            return new object[]
            {
                new { title = "ID", field = "id" },
                new { title = "Created", field = "createdAt" },
                new { title = "Updated", field = "updatedAt" },
                new { title = "Status", field = "isActive" },
                new { title = "Actions", field = "actions" }
            };
        }
    }
} 