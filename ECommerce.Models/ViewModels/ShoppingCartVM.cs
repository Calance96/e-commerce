using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Models.ViewModels
{
    /* This view model is for displaying the cart items at the shopping cart page */
    public class ShoppingCartVM
    {
        public IEnumerable<CartItem> CartItems { get; set; }
        public Order Order { get; set; }
    }
}
