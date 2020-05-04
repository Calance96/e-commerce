using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            return await _context.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> Get(long id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                _logger.LogWarning("Category of ID {CategoryId} not found", id);
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public async Task<Boolean> Add(Category category)
        {
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.Trim().ToLower());

            try
            {
                if (existingCategory == null)
                {
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    _logger.LogWarning("Attempt to add duplicate product category {@Category}", existingCategory);
                    return false;
                }
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new product category {@Category}", category);
                return false;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Boolean>> Update(long id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.Trim().ToLower());

            try
            {
                if (existingCategory == null)
                {
                    var oldCategoryValues = await _context.Categories.AsNoTracking().FirstAsync(c => c.Id == category.Id);

                    var categoryAuditTrail = new CategoryAuditTrail
                    {
                        CategoryId = oldCategoryValues.Id,
                        Name = oldCategoryValues.Name,
                        ActionTypeId = (long) SD.EntityActionType.Update,
                        PerformedBy = category.UpdatedBy,
                        PerformedDate = category.UpdatedAt
                    };

                    _context.CategoryAuditTrails.Add(categoryAuditTrail);
                    _context.Entry(category).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    _logger.LogWarning("Attempt to update product category {@Category} failed due to duplicate category {@ExistingCategory}", category, existingCategory);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product category {@Category}", category);
                return false;
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var categoryToDelete = await _context.Categories.FindAsync(id);

            if (categoryToDelete == null)
            {
                _logger.LogWarning("Attempt to delete non-existing category of ID {CategoryId}", id);
                return NotFound();
            } 

            try
            {
                _context.Categories.Remove(categoryToDelete);
                await _context.SaveChangesAsync();
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {@Category} from database", categoryToDelete);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }
    }
}
