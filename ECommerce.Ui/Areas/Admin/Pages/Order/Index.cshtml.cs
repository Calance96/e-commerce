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

        public string SearchTerm { get; set; }
        public string SearchCriterion { get; set; }

        public string StatusFilter { get; set; }

        public List<SelectListItem> SearchCriteria { get; set; }

        public async Task OnGetAsync(string searchString, string searchCriterion, string status, int? pageIndex)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusFilter = status;
            } else
            {
                StatusFilter = "All";
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                SearchTerm = searchString;
            }

            if (!string.IsNullOrEmpty(searchCriterion))
            {
                SearchCriterion = searchCriterion;
            }

            var OrdersFromDb = await _orderService.GetAllOrders(StatusFilter);
            
            SearchCriteria = GetSearchCriteriaList();

            switch (searchCriterion)
            {
                case "OrderID":
                    OrdersFromDb = OrdersFromDb.Where(o => o.Id.ToString() == searchString);
                    break;
                case "Customer":
                    OrdersFromDb = OrdersFromDb.Where(o => o.Name.ToLower().Contains(searchString.ToLower()));
                    break;
            }

            int pageSize = 10;
            Orders = await PaginatedList<Models.Order>.CreateAsync(OrdersFromDb.AsQueryable<Models.Order>(), pageIndex ?? 1, pageSize);
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
