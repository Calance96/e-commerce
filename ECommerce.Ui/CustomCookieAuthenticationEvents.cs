using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Ui
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly UserService _userService;

        public CustomCookieAuthenticationEvents(UserService userService)
        {
            _userService = userService;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userLockout = (await _userService.GetUserById(userId)).LockoutEnd;

            if (userLockout != null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            }
        }
    }
}
