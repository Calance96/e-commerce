using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Ui.Areas.Admin.Pages.Product
{
    public class EditModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService; // Used for retrieving the list of categories available
        private readonly IWebHostEnvironment _hostEnvironment;

        public EditModel(ProductService productService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public ProductViewModel ProductVM { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync(long id)
        {
            ProductVM = await _productService.GetById(id);
            await GetCategoryList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await GetCategoryList();
                return Page();
            }
            else if (HasDuplicateCategory())
            {
                await GetCategoryList();
                ModelState.AddModelError(string.Empty, "Duplicate category is not prohibited.");
                return Page();
            }

            ProductVM.CategoryIds = new List<long>();
            if (!string.IsNullOrEmpty(ProductVM.CategoryId_1))
            {
                ProductVM.CategoryIds.Add(long.Parse(ProductVM.CategoryId_1));
            }
            if (!string.IsNullOrEmpty(ProductVM.CategoryId_2))
            {
                ProductVM.CategoryIds.Add(long.Parse(ProductVM.CategoryId_2));
            }
            if (!string.IsNullOrEmpty(ProductVM.CategoryId_3))
            {
                ProductVM.CategoryIds.Add(long.Parse(ProductVM.CategoryId_3));
            }

            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                string filename = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);
                string webRootPath = _hostEnvironment.WebRootPath;
                string uploadPathPrefix = Path.Combine(webRootPath, "images", "products");

                if (ProductVM.Product.ImageUrl != null) // There is an existing image for this product
                {
                    string existingImagePath = Path.Combine(webRootPath, ProductVM.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(existingImagePath))
                    {
                        System.IO.File.Delete(existingImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(uploadPathPrefix, filename + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(fileStream);
                }

                ProductVM.Product.ImageUrl = @"\images\products\" + filename + extension;
            }
            ProductVM.Product.UpdatedAt = DateTime.Now;
            ProductVM.Product.UpdatedBy = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            
            var productUpdateSuccess = await _productService.Update(ProductVM);
            Message = productUpdateSuccess ? "successful" : "unsuccessful";

            return RedirectToPage("Index");
        }

        private async Task GetCategoryList()
        {
            var categories = await _categoryService.GetAll();
            CategoryList = categories.Select(item => new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            }).ToList();
        }

        private bool HasDuplicateCategory()
        {
            var hasDuplicate = false;

            if (!string.IsNullOrEmpty(ProductVM.CategoryId_2) && string.IsNullOrEmpty(ProductVM.CategoryId_3))
                hasDuplicate = ProductVM.CategoryId_1.Equals(ProductVM.CategoryId_2);
            else if (!string.IsNullOrEmpty(ProductVM.CategoryId_3) && string.IsNullOrEmpty(ProductVM.CategoryId_2))
                hasDuplicate = ProductVM.CategoryId_1.Equals(ProductVM.CategoryId_3);
            else if (!string.IsNullOrEmpty(ProductVM.CategoryId_2) && !string.IsNullOrEmpty(ProductVM.CategoryId_3))
                hasDuplicate = (ProductVM.CategoryId_1.Equals(ProductVM.CategoryId_2) ||
                                ProductVM.CategoryId_1.Equals(ProductVM.CategoryId_3) ||
                                ProductVM.CategoryId_2.Equals(ProductVM.CategoryId_3));

            return hasDuplicate;
        }
    }
}
