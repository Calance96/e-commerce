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
using Microsoft.Extensions.Logging;

namespace ECommerce.Ui.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        public string SearchTerm { get; set; }
        public string CategoryFilter { get; set; }
        public List<SelectListItem> Categories { get; set; }

        public IndexModel(ProductService productService, CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public PaginatedList<Product> Products { get; set; }

        public async Task OnGet(string searchString, int? pageIndex, string category)
        {
            var ProductsFromDb = await _productService.GetAll();
            CategoryFilter = category ?? "";

            Categories = new List<SelectListItem> {
                new SelectListItem {
                    Text = "All",
                    Value = "",
                    Selected = (CategoryFilter == "" ? true : false)                    
                }
            };

            Categories.AddRange((await _categoryService.GetAll()).Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name,
                Selected = (x.Name == category ? true : false)
            }).ToList());

            if (!string.IsNullOrEmpty(searchString))
            {
                SearchTerm = searchString;
                searchString = SearchTerm.ToLower();
                ProductsFromDb = ProductsFromDb.Where(p => p.Name.ToLower().Contains(searchString));
            }

            if (!string.IsNullOrEmpty(CategoryFilter))
            {
                ProductsFromDb = ProductsFromDb.Where(p => p.Category.Name == category);
            }

            int pageSize = 8;
            Products = await PaginatedList<Product>.CreateAsync(ProductsFromDb.AsQueryable<Product>(), pageIndex ?? 1, pageSize);

        }
    }
}
