using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve all users from the database based on role. 
        /// </summary>
        /// <param name="role">Role string, e.g. Admin, Customer</param>
        /// <returns></returns>
        [HttpGet("{role}")]
        public async Task<IEnumerable<ApplicationUser>> GetAll(string role)
        {
            var users = await _context.ApplicationUsers.OrderByDescending(x => x.CreatedAt).ToListAsync();
            var userRoles = await _context.UserRoles.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            foreach (var user in users)
            {
                var roleId = userRoles.FirstOrDefault(record => record.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(role => role.Id == roleId).Name;
            }

            switch (role)
            {
                case SD.ROLE_ADMIN:
                    users = users.Where(user => user.Role == SD.ROLE_ADMIN).ToList();
                    break;
                case SD.ROLE_CUSTOMER:
                    users = users.Where(user => user.Role == SD.ROLE_CUSTOMER).ToList();
                    break;

            }

            return users;
        }

        /// <summary>
        /// Get the details of a specific user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpGet("info/{userId}")]
        public async Task<ActionResult<ApplicationUser>> Get(string userId)
        {
            var user = await _context.ApplicationUsers.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Couldn't find user of ID {UserId}", userId);
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Update an existing user in the database.
        /// </summary>
        /// <param name="user">ApplicationUser Object</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ActionResult> Update(ApplicationUser user)
        {
            var userFromDb = await _context.ApplicationUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == user.Id);

            if (userFromDb == null)
            {
                return NotFound();
            }

            try
            {
                _context.ApplicationUsers.Update(user);
                await _context.SaveChangesAsync();
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when updating user {@User} in database", user);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }
    }
}
