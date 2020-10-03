using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Ui.ViewComponents
{
    public class CartIconViewComponent : ViewComponent
    {
        private readonly CartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartIconViewComponent(CartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartItemCount = _httpContextAccessor.HttpContext.Session.GetInt32(SD.CART_SESSION_KEY);

            if (cartItemCount == null)
            {
                cartItemCount = await _cartService.GetItemsCount(_httpContextAccessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

                _httpContextAccessor.HttpContext.Session.SetInt32(SD.CART_SESSION_KEY, cartItemCount.Value);
            }

            return View(cartItemCount);
        }
    }
}
