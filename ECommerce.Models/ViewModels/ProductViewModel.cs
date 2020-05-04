using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerce.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }

        public List<string> Categories { get; set; }

        [Required(ErrorMessage = "Must set at least one category for the product.")]
        [MinLength(1)]
        public List<long> CategoryIds { get; set; }
    }
}
