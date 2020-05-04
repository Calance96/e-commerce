using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ECommerce.Models;
using System.Security.Claims;

namespace ECommerce.Ui.Areas.Admin.Pages.Category
{
    public class CreateModel : PageModel
    {
        private readonly CategoryService _categoryService;

        public CreateModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Models.Category Category { get; set; }

        [TempData]
        public string Message { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Category.CreatedBy = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Category.CreatedAt = DateTime.Now;
            var success = await _categoryService.Add(Category);
            if (success)
            {
                Message = "Category added successfully.";
            } 
            else
            {
                ErrorMessage = "Category cannot be added due to possible duplicate.";
            }
            return RedirectToPage("Index");
        }
    }
}
