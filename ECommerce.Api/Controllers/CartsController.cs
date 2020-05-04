using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartsController> _logger;

        public CartsController(ApplicationDbContext context, ILogger<CartsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<IEnumerable<CartItem>> GetAll(string userId)
        {
            return await _context.CartItems
                .Include(item => item.Product)
                .Include(item => item.ApplicationUser)
                .Where(record => record.UserId == userId)
                .ToListAsync();
        }

        [HttpGet("count/{userId}")]
        public async Task<ActionResult<Int32>> GetCount(string userId)
        {
            var IsUserExist = _context.ApplicationUsers.Any(u => u.Id == userId);
            Int32 count = 0;

            if (IsUserExist)
            {
                count = await _context.CartItems.CountAsync(x => x.UserId == userId);
            }
            else
            {
                count = -1;
                _logger.LogWarning("Attempt to retrieve cart item count for non-existing user [{UserId}]", userId);
            }

            return count;
        }

        [HttpGet("{userId}/{productId}")]
        public async Task<ActionResult<CartItem>> GetCartItemBasedOnUserIdAndProductId(string userId, long productId)
        {
            CartItem cartItemFromDb = await _context.CartItems
                                    .Include(item => item.Product)
                                    .FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId);

            if (cartItemFromDb == null)
            {
                return NotFound();
            }

            return cartItemFromDb;
        }

        [HttpGet("details/{cartId}")]
        public async Task<ActionResult<CartItem>> GetCartItemBasedOnCartId(long cartId)
        {
            CartItem cartItemFromDb = await _context.CartItems
                                    .Include(item => item.Product)
                                    .FirstOrDefaultAsync(item => item.Id == cartId);

            if (cartItemFromDb == null)
            {
                return NotFound();
            }

            return cartItemFromDb;
        }

        [HttpPost]
        public async Task<ActionResult> Add(CartItem cartItem)
        {
            try
            {
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User {userId} add product {@Product} to cart failed", cartItem.UserId, cartItem.Product);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError), ex);
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CartItem cartItem)
        {
            if (id != cartItem.Id)
            {
                return BadRequest();
            }

            try
            {
                _context.Entry(cartItem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            } 
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User {userId} update product {productId} in cart failed", cartItem.UserId, cartItem.ProductId);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError), ex);
            }

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

            try
            {
                _context.CartItems.Remove(cartItemToDelete);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User {userId} remove product {productId} from cart failed", cartItemToDelete.UserId, cartItemToDelete.ProductId);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError), ex);
            }

            return NoContent();
        }
    }
}
