using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Ui.Areas.Account.Pages.Profile
{
    public class UpdateModel : PageModel
    {
        private readonly UserService _userService;

        public UpdateModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ProfileUpdateModel Input { get; set; }

        public async Task OnGetAsync()
        {
            Input = await GetUserInfo();
        }

        [TempData]
        public string ProfileUpdateMessage { get; set; }

        private async Task<ProfileUpdateModel> GetUserInfo()
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var user = await _userService.GetUserById(userId);

            return new ProfileUpdateModel
            {
                Name = user.Name,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<ActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var user = await _userService.GetUserById(userId);

                user.Name = Input.Name;
                user.Address = Input.Address;
                user.PhoneNumber = Input.PhoneNumber;

                await _userService.Update(user);
                ProfileUpdateMessage = "Profile updated successfully!";
                return RedirectToPage();
            }
            Input = await GetUserInfo();
            return Page();
        }
    }
}
