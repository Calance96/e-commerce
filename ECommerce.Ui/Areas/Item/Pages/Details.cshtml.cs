using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
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

        [BindProperty]
        public ProductViewModel ProductVM { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task OnGet(long id)
        {
            ProductVM = await _productService.GetById(id);

            if (ProductVM != null)
            {
                var product = ProductVM.Product;
                CartItem = new CartItem
                {
                    Product = product,
                    ProductId = product.Id
                };
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync()
        {
            if (ModelState.IsValid)
            {
                var productVM = await _productService.GetById(CartItem.ProductId); // For the case where user tried
                if (!productVM.Product.IsAvailable)                                // to modify the HTML and add
                    return RedirectToPage();                                       // unavailable item to cart

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var currentUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartItem.UserId = currentUserId.Value;

                CartItem cartItemFromDb = await _cartService.GetCartItemByUserIdAndProductId(CartItem);
                bool addToCartSuccess = false;

                if (cartItemFromDb == null)
                {
                    addToCartSuccess = await _cartService.Add(CartItem);
                }
                else
                {
                    cartItemFromDb.Quantity += CartItem.Quantity;
                    addToCartSuccess = await _cartService.Update(cartItemFromDb);
                }

                if (addToCartSuccess)
                {
                    var cartItemsCount = await _cartService.GetItemsCount(currentUserId.Value);
                    HttpContext.Session.SetInt32(SD.CART_SESSION_KEY, cartItemsCount);
                    SuccessMessage = "Item added to cart successfully!";
                } 
                else
                {
                    ErrorMessage = "Add to cart failed.";
                }

                return LocalRedirect("/");
            }
            return RedirectToPage();
        }
    }
}
