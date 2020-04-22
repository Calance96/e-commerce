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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Account.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CartService _cartService;

        public LoginModel(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            CartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user != null)
                {

                    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, false);

                    if (result.Succeeded)
                    {
                        var cartItemsCount = await _cartService.GetItemsCount(user.Id);
                        HttpContext.Session.SetInt32(AppConstant.CART_SESSION_KEY, cartItemsCount);
                        HttpContext.Session.SetString(AppConstant.FULLNAME_SESSION_KEY, user.Name);
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        Message = "Incorrect credentials.";
                    }
                } 
                else
                {
                    Message = "User not found.";
                }
            }
            return Page();
        }
    }
}
