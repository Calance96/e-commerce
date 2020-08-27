using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Models.DTO;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context,
                              RoleManager<IdentityRole> roleManager,
                              UserManager<ApplicationUser> userManager,
                              ILogger<AuthController> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Verify the combination of email and password to grant user access to use E-Mall.
        /// </summary>
        /// <param name="loginInput">
        /// Provides the necessary email and password combination for authentication
        /// </param>
        /// <returns>
        /// An authentication result object that contains a status code. If authentication is
        /// successful, a user is included for creation of claims. Otherwise, an error message
        /// is included to indicate the failure reason.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
                    _logger.LogWarning("User {UserId} provided the wrong password", user.Id);
                    user = null;
                    statusCode = SD.StatusCode.UNAUTHORIZED;
                    messages.Add("Invalid credentials.");
                }
            }
            else
            {
                _logger.LogWarning("Attempt to login with {Email} which doesn't exist", loginInput.Email);
                user = null;
                statusCode = SD.StatusCode.NOTFOUND;
                messages.Add("User not found.");
            }

            return new AuthResult
            {
                ApplicationUser = user,
                StatusCode = statusCode,
                Message = messages
            };
        }

        /// <summary>
        /// Take input from user to register a new account
        /// </summary>
        /// <param name="registerInput">
        /// Provide necessary information for registering a new user in the database
        /// </param>
        /// <returns>
        /// Similarly return an authentication result object that contains a status 
        /// code to indicate success or failure.
        /// </returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResult>> Register(RegisterViewModel registerInput)
        {
            try
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = registerInput.Email,
                    Email = registerInput.Email,
                    Address = registerInput.Address,
                    Name = registerInput.Name,
                    PhoneNumber = registerInput.PhoneNumber,
                    Role = registerInput.Role,
                    CreatedAt = DateTime.Now,
                };

                var createUserResult = await _userManager.CreateAsync(user, registerInput.Password);

                if (createUserResult.Succeeded)
                {
                    user.Role ??= SD.ROLE_CUSTOMER;
                    await _userManager.AddToRoleAsync(user, user.Role);
                    user.CreatedBy = user.Id;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User {Email} has been created with role as {Role}", user.UserName, user.Role);

                    return await Login(new LoginViewModel
                    {
                        Email = registerInput.Email,
                        Password = registerInput.Password,
                        RememberMe = false
                    });
                }
                else
                {
                    _logger.LogWarning("User {Email} failed to be registered", user.UserName);

                    return new AuthResult
                    {
                        StatusCode = SD.StatusCode.BAD_REQUEST,
                        Message = new List<string>(createUserResult.Errors.Select(e => e.Description))
                    };
                }
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed with register inputs {@RegisterInput} with exception", registerInput);
            }

            var errorMessage = new List<string>
            {
                "Registration cannot be processed. Please try again later or contact E-Mall for further information."
            };

            return new AuthResult
            {
                StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError),
                Message = errorMessage
            };
        }


        /// <summary>
        /// Accepts a combination of user ID, current password and new password for password change.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Returns boolean to indicate success or failure.
        /// </returns>
        [HttpPost("password_change")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<Boolean> ChangePassword(ChangePasswordModel input)
        {
            ApplicationUser user = null;
            try
            {
                user = await _userManager.FindByIdAsync(input.UserId);
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);

                if (changePasswordResult.Succeeded)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password change by User {UserId} failed inputs {@RegisterInput} with exception", user.Id, input);
            }
            return false;
        }
    }
}
