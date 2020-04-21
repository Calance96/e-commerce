using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Product.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;
        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        public IEnumerable<Models.Product> Products { get; set; }

        public async Task OnGet()
        {
            Products = await _productService.GetAll();
        }

        public async Task<IActionResult> OnPostDelete(long id)
        {
            try
            {
                await _productService.Delete(id);
                return RedirectToPage("Index");
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
