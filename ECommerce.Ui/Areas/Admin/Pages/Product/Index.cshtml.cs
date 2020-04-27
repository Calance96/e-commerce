using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Ui.Areas.Admin.Pages.Product
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        public IndexModel(ProductService productService, CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public string SearchTerm { get; set; }
        public string CategoryFilter { get; set; }
        public List<SelectListItem> Categories { get; set; }

        public PaginatedList<Models.Product> Products { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGet(string searchString, string category, int? pageIndex)
        {
            var ProductsFromDb = await _productService.GetAll();
            CategoryFilter = category ?? "All";
            SearchTerm = searchString ?? "";

            Categories = new List<SelectListItem> {
                new SelectListItem
                {
                    Text = "All",
                    Value = "All",
                    Selected = (CategoryFilter == "All" ? true : false)
                } 
            };

            Categories.AddRange((await _categoryService.GetAll()).Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name,
                Selected = (x.Name == category ? true : false)
            }).ToList());

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                ProductsFromDb = ProductsFromDb.Where(p => p.Name.ToLower().Contains(SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(category) && CategoryFilter != "All")
            {
                ProductsFromDb = ProductsFromDb.Where(p => p.Category.Name == category);
            }

            int pageSize = 8;
            Products = await PaginatedList<Models.Product>.CreateAsync(ProductsFromDb.AsQueryable<Models.Product>(), pageIndex ?? 1, pageSize);
        }

        public async Task<IActionResult> OnPostDelete(long id, string searchString, string category, int pageIndex)
        {
            try
            {
                var success = await _productService.Delete(id);
                if (success)
                {
                    Message = "successful";
                }
                else
                {
                    Message = "unsuccessful";
                }
                return RedirectToPage("Index", new { searchString, category, pageIndex });
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
