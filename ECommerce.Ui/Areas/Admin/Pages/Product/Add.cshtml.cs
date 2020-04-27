using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Ui.Areas.Admin.Pages.Product
{
    public class AddModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService; // Used for retrieving the list of categories available
        private readonly IWebHostEnvironment _hostEnvironment;
        private IEnumerable<Models.Category> categories; 

        public AddModel(ProductService productService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public ProductViewModel ProductVM { get; set; }

        public async Task OnGetAsync()
        {
            categories = await _categoryService.GetAll();
            ProductVM = new ProductViewModel
            {
                Product = new Models.Product(),
                CategoryList = categories.Select(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                }).ToList()
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ProductVM.CategoryList = categories.Select(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                }).ToList();

                return Page();
            }

            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                string filename = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);
                string webRootPath = _hostEnvironment.WebRootPath;
                string uploadPathPrefix = Path.Combine(webRootPath, "images", "products");
                

                using (var fileStream = new FileStream(Path.Combine(uploadPathPrefix, filename+extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(fileStream);
                }
                ProductVM.Product.ImageUrl = @"\images\products\" + filename + extension;
            }
            ProductVM.Product.IsAvailable = true;
            await _productService.Add(ProductVM.Product);
            return RedirectToPage("Index");
        }
    }
}
