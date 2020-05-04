using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Admin.Pages.Category
{
    public class EditModel : PageModel
    {
        private readonly CategoryService _categoryService;

        public EditModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Models.Category Category { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            Category = await _categoryService.GetById(id);
            
            if (Category == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Category.UpdatedAt = DateTime.Now;
            Category.UpdatedBy = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var success = await _categoryService.Update(Category);
            if (!success)
            {
                ErrorMessage = "Duplicate category name is prohibited.";
            }
            return RedirectToPage("Index");
        }
    }
}
