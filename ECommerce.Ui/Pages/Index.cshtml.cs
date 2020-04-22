using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ECommerce.Ui.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;

        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        public IEnumerable<Product> Products { get; set; }

        public async Task OnGet()
        {
            Products = await _productService.GetAll();
        }
    }
}
