using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Customer.Pages.Order
{
    public class DetailsModel : PageModel
    {
        private readonly OrderService _orderService;

        public DetailsModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty]
        public OrderDetailsVM OrderDetails { get; set; }

        public async Task OnGet(long orderId)
        {
            OrderDetails = await _orderService.GetOrderDetailsByOrderId(orderId);
        }

        public async Task<IActionResult> OnPostCompleteOrderAsync()
        {
            Models.Order orderFromDb = await _orderService.GetOrderSummaryByOrderId(OrderDetails.Order.Id);
            orderFromDb.OrderStatus = SD.OrderStatus.COMPLETE;
            await _orderService.Update(orderFromDb);
            return RedirectToPage();
        }
    }
}
