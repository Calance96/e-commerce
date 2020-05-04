using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models.ViewModels;
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

        public List<SelectListItem> Categories { get; set; }

        private const int PAGE_SIZE = 8;

        public string SearchTerm { get; set; }
        public string CategoryFilter { get; set; }

        public PaginatedList<Models.Product> Products { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGet(string searchString, string category, int? pageIndex)
        {
            var ProductsFromDb = await _productService.GetAll();
            CategoryFilter = category?.Trim() ?? "All";
            SearchTerm = searchString?.Trim() ?? "";

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
                ProductsFromDb = ProductsFromDb.Where(p => p.Product.Name.ToLower().Contains(SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(category) && CategoryFilter != "All")
            {
                ProductsFromDb = ProductsFromDb.Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));
            }

            Products = PaginatedList<Models.Product>.Create(ProductsFromDb.Select(x => x.Product).AsQueryable<Models.Product>(), pageIndex ?? 1, PAGE_SIZE);
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
