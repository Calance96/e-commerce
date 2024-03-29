using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Customer.Pages.Order
{
    public class IndexModel : PageModel
    {
        private readonly OrderService _orderService;

        public IndexModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public PaginatedList<Models.Order> Orders { get; set; }

        private const int PAGE_SIZE = 10;

        public string SearchTerm { get; set; }
        public string SearchCriterion { get; set; }

        public string StatusFilter { get; set; }

        public async Task OnGet(string searchString, string status, int? pageIndex)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            StatusFilter = status?.Trim() ?? "All";

            var OrdersFromDb = await _orderService.GetAllOrdersForUserId(userId, StatusFilter);

            if (!string.IsNullOrEmpty(searchString))
            {
                SearchTerm = searchString.Trim();
                OrdersFromDb = OrdersFromDb.Where(o => o.Id.ToString() == SearchTerm);
            }

            Orders = PaginatedList<Models.Order>.Create(OrdersFromDb.AsQueryable<Models.Order>(), pageIndex ?? 1, PAGE_SIZE);
        }
    }
}
