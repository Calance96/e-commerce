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

        public string SearchTerm { get; set; }
        public string SearchCriterion { get; set; }

        public string StatusFilter { get; set; }


        public async Task OnGet(string searchString, string status, int? pageIndex)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (!string.IsNullOrEmpty(status))
            {
                StatusFilter = status;
            }
            else
            {
                StatusFilter = "All";
            }

            var OrdersFromDb = await _orderService.GetAllOrdersForUserId(userId, StatusFilter);

            if (!string.IsNullOrEmpty(searchString))
            {
                SearchTerm = searchString;
                OrdersFromDb = OrdersFromDb.Where(o => o.Id.ToString() == searchString.Trim());
            }

            int pageSize = 10;
            Orders = await PaginatedList<Models.Order>.CreateAsync(OrdersFromDb.AsQueryable<Models.Order>(), pageIndex ?? 1, pageSize);
        }
    }
}
