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

        public List<long> CategoryIds { get; set; }

        [Required(ErrorMessage = "One least one category must be set for the product.")]
        public string CategoryId_1 { get; set; }

        public string CategoryId_2 { get; set; }

        public string CategoryId_3 { get; set; }
    }
}
