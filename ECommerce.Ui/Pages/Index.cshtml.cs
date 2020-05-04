using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
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

        public bool ShowAvailableOnly { get; set; }

        public List<SelectListItem> Categories { get; set; }

        private const int PAGE_SIZE = 8;

        public IndexModel(ProductService productService, CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public PaginatedList<ProductViewModel> ProductVMs { get; set; }

        public async Task OnGet(string availableOnly, string searchString, int? pageIndex, string category)
        {
            ShowAvailableOnly = availableOnly == null ? false : true;
            CategoryFilter = category ?? "";

            var ProductsFromDb = await _productService.GetAll();
            
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
                ProductsFromDb = ProductsFromDb.Where(p => p.Product.Name.ToLower().Contains(searchString));
            }

            if (!string.IsNullOrEmpty(CategoryFilter))
            {
                ProductsFromDb = ProductsFromDb.Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));
            }

            if (ShowAvailableOnly)
            {
                ProductsFromDb = ProductsFromDb.Where(x => x.Product.IsAvailable);
            }

            ProductVMs = PaginatedList<ProductViewModel>.Create(ProductsFromDb.AsQueryable<ProductViewModel>(), pageIndex ?? 1, PAGE_SIZE);
        }
    }
}
