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
    [Route("api/[controller]")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<Int32>> GetCount(string userId)
        {
            Int32 count = await _context.CartItems.CountAsync(x => x.UserId == userId);
            return count;
        }

        [HttpGet("{userId}/{productId}")]
        public async Task<ActionResult<CartItem>> Get(string userId, long productId)
        {
            CartItem cartItemFromDb = await _context.CartItems.Include(item => item.Product).FirstOrDefaultAsync(item =>
                item.UserId == userId && item.ProductId == productId
            );

            if (cartItemFromDb == null)
            {
                return NotFound();
            }

            return cartItemFromDb;
        }

        [HttpPost]
        public async Task<ActionResult<CartItem>> Add(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = cartItem.Id }, cartItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CartItem cartItem)
        {
            if (id != cartItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(cartItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var cartItemToDelete = await _context.CartItems.FindAsync(id);

            if (cartItemToDelete == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItemToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
