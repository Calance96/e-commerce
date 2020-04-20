using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Category.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CategoryService _categoryService;

        public IEnumerable<Models.Category> Categories { get; set; } = new List<Models.Category>();

        public IndexModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task OnGet()
        {
            Categories = await _categoryService.GetAll();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _categoryService.Delete(id);
                return RedirectToPage("Index");
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
