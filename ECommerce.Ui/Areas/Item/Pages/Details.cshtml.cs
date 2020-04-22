using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Item.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly CartService _cartService;

        public DetailsModel(ProductService productService, CartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        [BindProperty]
        public CartItem CartItem { get; set; }

        public async Task OnGet(long id)
        {
            Product product = await _productService.GetById(id);

            CartItem = new CartItem
            {
                Product = product,
                ProductId = product.Id
            };
        }

        public async Task<IActionResult> OnPostAddToCartAsync()
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var currentUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartItem.UserId = currentUserId.Value;

                CartItem cartItemFromDb = await _cartService.GetCartItemByUserIdAndProductId(CartItem);

                if (cartItemFromDb == null)
                {
                    await _cartService.Add(CartItem);
                }
                else
                {
                    cartItemFromDb.Quantity += CartItem.Quantity;
                    await _cartService.Update(cartItemFromDb);
                }

                var cartItemsCount = await _cartService.GetItemsCount(currentUserId.Value);

                HttpContext.Session.SetInt32(AppConstant.CART_SESSION_KEY, cartItemsCount);

                return LocalRedirect("/");
            }
            return Page();
        }
    }
}
