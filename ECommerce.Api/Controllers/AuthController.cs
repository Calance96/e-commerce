using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Models.DTO;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(ApplicationDbContext context,
                              RoleManager<IdentityRole> roleManager,
                              UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("login")]
        public string CheckAuthApi()
        {
            return "Auth API is running normally";
        }

        [HttpPost]
        [Route("login")]
        public async Task<AuthResult> Login(LoginViewModel loginInput)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(loginInput.Email);
            int statusCode = SD.StatusCode.OK;
            var messages = new List<string>();

            if (user != null)
            {
                bool isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginInput.Password);

                if (isPasswordCorrect)
                {
                    var userRoleId = (await _context.UserRoles.FirstOrDefaultAsync(u => u.UserId == user.Id)).RoleId;
                    var userRole = (await _context.Roles.FirstOrDefaultAsync(r => r.Id == userRoleId)).Name;
                    user.Role = userRole;
                }
                else
                {
                    user = null;
                    statusCode = SD.StatusCode.UNAUTHORIZED;
                    messages.Add("Invalid credentials");
                }
            }
            else
            {
                user = null;
                statusCode = SD.StatusCode.NOTFOUND;
                messages.Add("User not found");
            }

            return new AuthResult
            {
                ApplicationUser = user,
                StatusCode = statusCode,
                Message = messages
            }; ;
        }

        //[HttpPost]
        //[Route("register")]
        //public async Task<ActionResult<AuthResult>> Register(RegisterViewModel registerInput)
        //{
        //    ApplicationUser user = new ApplicationUser
        //    {
        //        UserName = registerInput.Email,
        //        Email = registerInput.Email,
        //        Address = registerInput.Address,
        //        Name = registerInput.Name,
        //        PhoneNumber = registerInput.PhoneNumber,
        //        Role = registerInput.Role
        //    };

        //    var createUserResult = await _userManager.CreateAsync(user, registerInput.Password);

        //    if (createUserResult.Succeeded)
        //    {
        //        if (!await _roleManager.RoleExistsAsync(SD.ROLE_ADMIN))
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN));
        //        }
        //        if (!await _roleManager.RoleExistsAsync(SD.ROLE_CUSTOMER))
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_CUSTOMER));
        //        }


        //        await _userManager.AddToRoleAsync(user, user.Role ?? SD.ROLE_CUSTOMER);

        //        return await Login(new LoginViewModel
        //        {
        //            Email = registerInput.Email,
        //            Password = registerInput.Password
        //        });
        //    }
        //    else
        //    {
        //        return new AuthResult
        //        {
        //            StatusCode = SD.StatusCode.BAD_REQUEST,
        //            Message = new List<string>(createUserResult.Errors.Select(e => e.Description))
        //        };
        //    }
        //}
    }
}
