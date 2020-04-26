using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Customer.Pages.Profile
{
    public class ChangePasswordModel : PageModel
    {
        private readonly AuthService _authService;

        public ChangePasswordModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public Models.ViewModels.ChangePasswordModel Input { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                Input.UserId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var isSuccess = await _authService.ChangePassword(Input);

                if (!isSuccess)
                {
                    ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                } 
                else
                {
                    SuccessMessage = "Password changed successfully!";
                    return LocalRedirect("/");
                }
            } 
            return Page();
        }
    }
}
