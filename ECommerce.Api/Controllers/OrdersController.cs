using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
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

        [HttpGet("{userId}/{status}")]
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
                case AppConstant.OrderStatus.PENDING:
                    orders = orders.Where(order => order.PaymentStatus == AppConstant.PaymentStatus.PENDING ||
                                            order.OrderStatus == AppConstant.OrderStatus.APPROVED);
                    break;
                case AppConstant.OrderStatus.PROCESSING:
                    orders = orders.Where(order => order.OrderStatus == AppConstant.OrderStatus.PROCESSING);
                    break;
                case AppConstant.OrderStatus.SHIPPED:
                    orders = orders.Where(order => order.OrderStatus == AppConstant.OrderStatus.SHIPPED);
                    break;
                case AppConstant.OrderStatus.COMPLETE:
                    orders = orders.Where(order => order.OrderStatus == AppConstant.OrderStatus.COMPLETE);
                    break;
                case AppConstant.OrderStatus.CANCELLED:
                    orders = orders.Where(order => order.OrderStatus == AppConstant.OrderStatus.CANCELLED ||
                                            order.OrderStatus == AppConstant.OrderStatus.REFUNDED ||
                                            order.OrderStatus == AppConstant.PaymentStatus.REJECTED);
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

        [HttpPut("{orderId}")]
        public async Task<IActionResult> Update(long orderId, Order order)
        {
            if (orderId != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
