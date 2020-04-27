using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Ui.Areas.Admin.Pages.Management
{
    public class IndexModel : PageModel
    {
        private readonly UserService _userService;

        public IndexModel(UserService userService)
        {
            _userService = userService;
        }

        public PaginatedList<ApplicationUser> Users { get; set; }

        public string SearchTerm { get; set; }
        public string SearchCriterion { get; set; }
        public string RoleFilter { get; set; }

        public List<SelectListItem> SearchCriteria { get; set; }

        public async Task OnGetAsync(string searchString, string searchCriterion, string role, int? pageIndex)
        {
            RoleFilter = role ?? "all";
            SearchTerm = searchString ?? "";
            SearchCriterion = searchCriterion ?? "";

            var UsersFromDb = await _userService.GetAllUsers(RoleFilter);

            SearchCriteria = GetSearchCriteriaList();

            switch (searchCriterion)
            {
                case "Email":
                    UsersFromDb = UsersFromDb.Where(u => u.Email.ToLower().Contains(searchString.ToLower()));
                    break;
                case "Name":
                    UsersFromDb = UsersFromDb.Where(u => u.Name.ToLower().Contains(searchString.ToLower()));
                    break;
            }

            int pageSize = 10;
            Users = await PaginatedList<ApplicationUser>.CreateAsync(UsersFromDb.AsQueryable<ApplicationUser>(), pageIndex ?? 1, pageSize);
        }

        private List<SelectListItem> GetSearchCriteriaList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Email",
                    Value = "Email",
                    Selected = (SearchCriterion == "Email" ? true : false)
                },
                new SelectListItem
                {
                    Text = "User's Name",
                    Value = "Name",
                    Selected = (SearchCriterion == "Name" ? true : false)
                }
            };
        }
    }
}
