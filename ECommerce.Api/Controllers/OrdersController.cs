using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all orders of a specific status.
        /// </summary>
        /// <param name="status">Order status, e.g. Approved, Processing, Shipped, Complete, Cancelled</param>
        /// <returns></returns>
        [HttpGet("{status}")]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<Order>> GetAllOrdersOfStatus(string status)
        {
            IEnumerable<Order> orders = await _context.Orders
                                        .OrderByDescending(order => order.Id)
                                        .ToListAsync();

            orders = FilterOrderStatus(status, orders);
            return orders;
        }

        /// <summary>
        /// Get all orders of a specific status for a specific user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="status">Order status, e.g. Approved, Processing, Shipped, Complete, Cancelled</param>
        /// <returns></returns>
        [HttpGet("user/{userId}/{status}")]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<Order>> GetAllOrdersForUserById(string userId, string status)
        {
            IEnumerable<Order> orders = await _context.Orders
                                        .Where(order => order.UserId == userId)
                                        .OrderByDescending(order => order.Id)
                                        .ToListAsync();
            orders = FilterOrderStatus(status, orders);
            return orders;
        }

        private static IEnumerable<Order> FilterOrderStatus(string status, IEnumerable<Order> orders)
        {
            switch (status)
            {
                case SD.OrderStatus.APPROVED:
                    orders = orders.Where(order => order.OrderStatus == SD.OrderStatus.APPROVED);
                    break;
                case SD.OrderStatus.PROCESSING:
                    orders = orders.Where(order => order.OrderStatus == SD.OrderStatus.PROCESSING);
                    break;
                case SD.OrderStatus.SHIPPED:
                    orders = orders.Where(order => order.OrderStatus == SD.OrderStatus.SHIPPED);
                    break;
                case SD.OrderStatus.COMPLETE:
                    orders = orders.Where(order => order.OrderStatus == SD.OrderStatus.COMPLETE);
                    break;
                case SD.OrderStatus.CANCELLED:
                    orders = orders.Where(order => order.OrderStatus == SD.OrderStatus.CANCELLED ||
                                            order.OrderStatus == SD.OrderStatus.REFUNDED ||
                                            order.OrderStatus == SD.PaymentStatus.REJECTED);
                    break;
            }

            return orders;
        }

        /// <summary>
        /// Retrieve a specific order.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns></returns>
        [HttpGet("summary/{orderId}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Order>> GetOrderById(long orderId)
        {
            Order order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                _logger.LogWarning("Couldn't find summary for order of ID {OrderId}", orderId);
                return NotFound();
            }

            return order;
        }

        /// <summary>
        /// Retrieve a specific order and its order items.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns></returns>
        [HttpGet("details/{orderId}")]
        [ProducesResponseType(typeof(OrderDetailsVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDetailsVM>> GetOrderDetailsByOrderId(long orderId)
        {
            Order order = await _context.Orders.Include(order => order.User).FirstOrDefaultAsync(order => order.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Couldn't find details for order of ID {OrderId}", orderId);
                return NotFound();
            }

            OrderDetailsVM orderDetailsVM = new OrderDetailsVM
            {
                Order = order,
                OrderItems = await _context.OrderItems
                            .Include(item => item.Product)
                            .Where(item => item.OrderId == orderId)
                            .ToListAsync()
            };

            return orderDetailsVM;

        }

        /// <summary>
        /// Create an order from a user's shopping cart upon place order.
        /// </summary>
        /// <param name="shoppingCart">Shopping Cart Object</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Order>> Create(ShoppingCartVM shoppingCart)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var newOrder = shoppingCart.Order;
                    await _context.Orders.AddAsync(newOrder);
                    await _context.SaveChangesAsync();

                    foreach (var item in shoppingCart.CartItems)
                    {
                        OrderItem orderItem = new OrderItem
                        {
                            OrderId = newOrder.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        await _context.OrderItems.AddAsync(orderItem);
                    }


                    IEnumerable<CartItem> cartItems = await _context.CartItems
                        .Where(x => x.UserId == shoppingCart.Order.UserId)
                        .ToListAsync();
                    _context.RemoveRange(cartItems);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return newOrder;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating order for user {UserId}", shoppingCart.Order.UserId);
                    return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
                }
            }
        }

        /// <summary>
        /// Update an existing order.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPut("{orderId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long orderId, Order order)
        {
            if (orderId != order.Id)
            {
                return BadRequest();
            }

            var orderFromDb = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(order => order.Id == orderId);

            if (orderFromDb == null)
            {
                return NotFound();
            }

            try
            {
                if (order.OrderActionId > 0)
                {
                    var orderAuditTrail = new OrderAuditTrail
                    {
                        OrderId = order.Id,
                        OrderActionId = order.OrderActionId,
                        PerformedBy = order.UpdatedBy,
                        PerformedDate = order.UpdatedAt
                    };
                    _context.OrderAuditTrails.Add(orderAuditTrail);
                }
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order of ID {OrderId}", order.Id);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }
    }
}
