using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Admin.Pages.Category
{
    public class IndexModel : PageModel
    {
        private readonly CategoryService _categoryService;

        public PaginatedList<Models.Category> Categories { get; set; }

        public string SearchTerm { get; set; }

        private const int PAGE_SIZE = 10;

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task OnGet(string searchString, int? pageIndex)
        {
            SearchTerm = searchString?.Trim() ?? "";

            var CategoriesFromDb = await _categoryService.GetAll();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                searchString = searchString.ToLower();
                CategoriesFromDb = CategoriesFromDb.Where(c => c.Name.ToLower().Contains(searchString));
            }

            Categories = await PaginatedList<Models.Category>.CreateAsync(CategoriesFromDb.AsQueryable<Models.Category>(), pageIndex ?? 1, PAGE_SIZE);            
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string searchString, int? pageIndex)
        {
            var success = await _categoryService.Delete(id);
            if (!success)
            {
                ErrorMessage = "Category cannot be deleted as there are products belong to this category.";
            } 
            else
            {
                if (Categories.Count() == 1 && pageIndex > 1) // The last item at this page is deleted, so let's move to the previous page instead
                {
                    pageIndex--;
                }
            }

            return RedirectToPage("Index", new { searchString, pageIndex });
        }
    }
}
