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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Account.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AuthService _authService;

        public RegisterModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; }

        [TempData]
        public string RegisterSuccessMessage { get; set; }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                var authResult = await _authService.Register(Input);
                
                switch (authResult.StatusCode)
                {
                    case SD.StatusCode.OK:
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, authResult.ApplicationUser.Id),
                            new Claim(ClaimTypes.GivenName, authResult.ApplicationUser.Name),
                            new Claim(ClaimTypes.Email, authResult.ApplicationUser.Email),
                            new Claim(ClaimTypes.Role, authResult.ApplicationUser.Role),
                            new Claim(ClaimTypes.MobilePhone, authResult.ApplicationUser.PhoneNumber),
                        };

                        //var claimsIdentity = new ClaimsIdentity(claims, "Password");
                        //var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                        //{
                        //    IsPersistent = true,
                        //    RedirectUri = returnUrl
                        //});
                        RegisterSuccessMessage = $"Registration success. Kindly proceed to login.";

                        return RedirectToPage();
                    case SD.StatusCode.BAD_REQUEST:
                    default:
                        foreach (var error in authResult.Message)
                        {
                            ModelState.AddModelError(string.Empty, error);
                        }
                        break;
                }
            }
            return Page();
        }
    }
}
