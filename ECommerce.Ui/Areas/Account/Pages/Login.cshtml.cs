using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Account.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;
        private readonly CartService _cartService;

        public LoginModel(
           AuthService authService,
            CartService cartService)
        {
            _authService = authService;
            _cartService = cartService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            Message = "";
            if (ModelState.IsValid)
            {
                var authResult = await _authService.Login(Input);

                switch (authResult.StatusCode)
                {
                    case SD.StatusCode.OK:
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, authResult.ApplicationUser.Id),
                            new Claim(ClaimTypes.Name, authResult.ApplicationUser.Name),
                            new Claim(ClaimTypes.Email, authResult.ApplicationUser.Email),
                            new Claim(ClaimTypes.Role, authResult.ApplicationUser.Role),
                            new Claim(ClaimTypes.MobilePhone, authResult.ApplicationUser.PhoneNumber),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "Password");
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                        {
                            IsPersistent = true,
                            RedirectUri = returnUrl
                        });

                        var cartItemsCount = await _cartService.GetItemsCount(authResult.ApplicationUser.Id);

                        HttpContext.Session.SetInt32(SD.CART_SESSION_KEY, cartItemsCount);

                        return LocalRedirect(returnUrl);
                    case SD.StatusCode.NOTFOUND:
                    case SD.StatusCode.BAD_REQUEST:
                        Message = authResult.Message[0];
                        break;
                }
            }
            return Page();
        }
    }
}
