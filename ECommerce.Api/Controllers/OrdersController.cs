using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
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

        [HttpGet("{status}")]
        public async Task<IEnumerable<Order>> GetAllOrdersOfStatus(string status)
        {
            IEnumerable<Order> orders = await _context.Orders
                                        .OrderByDescending(order => order.Id)
                                        .ToListAsync();

            orders = FilterOrderStatus(status, orders);
            return orders;
        }

        [HttpGet("user/{userId}/{status}")]
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

        [HttpGet("summary/{orderId}")]
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

        [HttpGet("details/{orderId}")]
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

        [HttpPost]
        public async Task<ActionResult<Order>> Create(ShoppingCartVM shoppingCart)
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
                return newOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order for user {UserId}", shoppingCart.Order.UserId);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> Update(long orderId, Order order)
        {
            if (orderId != order.Id)
            {
                return BadRequest();
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
