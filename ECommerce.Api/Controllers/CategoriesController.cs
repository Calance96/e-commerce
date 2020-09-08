using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

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

        /// <summary>
        /// Retrieve all existing categories in the database.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("GetAll")]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            return await _context.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        }
        
        /// <summary>
        /// Retrieve a specific category.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("Get")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Add a new category to the database. This checks for duplicate category name.
        /// </summary>
        /// <param name="category">Category Object</param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Add")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Update an existing category in the database. This checks for duplicate category name.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="category">Category Object</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [SwaggerOperation("Update")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Remove an existing category from database.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpDelete("{userId}/{id}")]
        [SwaggerOperation("Delete")]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id, string userId)
        {
            var categoryToDelete = await _context.Categories.FindAsync(id);

            if (categoryToDelete == null)
            {
                _logger.LogWarning("Attempt to delete non-existing category of ID {CategoryId}", id);
                return NotFound();
            } 

            try
            {
                var categoryAuditTrail = new CategoryAuditTrail
                {
                    CategoryId = categoryToDelete.Id,
                    Name = categoryToDelete.Name,
                    ActionTypeId = (long)SD.EntityActionType.Delete,
                    PerformedBy = userId,
                    PerformedDate = DateTime.Now
                };
                _context.CategoryAuditTrails.Add(categoryAuditTrail);
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
