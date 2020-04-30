using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Admin.Pages.Management
{
    public class AddModel : PageModel
    {
        private readonly AuthService _authService;

        public AddModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                Input.Role = SD.ROLE_ADMIN; 

                var result = await _authService.Register(Input);

                switch (result.StatusCode)
                {
                    case SD.StatusCode.OK:
                        return RedirectToPage("./Index");

                    case SD.StatusCode.BAD_REQUEST:
                        foreach (var error in result.Message)
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
