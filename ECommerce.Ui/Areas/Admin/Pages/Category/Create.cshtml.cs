using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ECommerce.Models;

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

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _categoryService.Add(Category);
            Message = "Category added successfully";
            return RedirectToPage("Index");
        }
    }
}
