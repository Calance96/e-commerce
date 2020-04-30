using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            return await _context.Categories.OrderBy(x => x.Name).ToListAsync();
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> Get(long id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public async Task<Boolean> Add(Category category)
        {
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.Trim().ToLower());

            if (existingCategory == null)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            } 
            else
            {
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

            if (existingCategory == null)
            {
                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            } 
            else
            {
                return false;
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var categoryToDelete = await _context.Categories.FindAsync(id);

            if (categoryToDelete == null)
            {
                return NotFound();
            } 
            else if (_context.Products.Any(p => p.CategoryId == id))
            {
                return BadRequest();
            }

            _context.Categories.Remove(categoryToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
