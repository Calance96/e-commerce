using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Ui.Areas.Admin.Pages.Order
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

        public List<SelectListItem> SearchCriteria { get; set; }

        public async Task OnGetAsync(string searchString, string searchCriterion, string status, int? pageIndex)
        {
            StatusFilter = status?.Trim() ?? "All";
            SearchTerm = searchString?.Trim() ?? "";

            var OrdersFromDb = await _orderService.GetAllOrders(StatusFilter);
            
            SearchCriteria = GetSearchCriteriaList();

            if (!string.IsNullOrEmpty(searchCriterion))
            {
                SearchCriterion = searchCriterion;
                switch (searchCriterion)
                {
                    case "OrderID":
                        OrdersFromDb = OrdersFromDb.Where(o => o.Id.ToString() == searchString);
                        break;
                    case "Customer":
                        OrdersFromDb = OrdersFromDb.Where(o => o.Name.ToLower().Contains(searchString.ToLower()));
                        break;
                }
            }

            Orders = await PaginatedList<Models.Order>.CreateAsync(OrdersFromDb.AsQueryable<Models.Order>(), pageIndex ?? 1, PAGE_SIZE);
        }

        private List<SelectListItem> GetSearchCriteriaList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Order ID",
                    Value = "OrderID",
                    Selected = (SearchCriterion == "OrderID" ? true : false)
                },
                new SelectListItem
                {
                    Text = "Customer Name",
                    Value = "Customer",
                    Selected = (SearchCriterion == "Customer" ? true : false)
                }
            };
        }
    }
}
