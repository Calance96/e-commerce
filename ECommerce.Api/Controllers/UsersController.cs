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

            if (role == SD.ROLE_ADMIN)
            {
                users = users.Where(user => user.Role == SD.ROLE_ADMIN).ToList();
            } 
            else if (role == SD.ROLE_CUSTOMER)
            {
                users = users.Where(user => user.Role == SD.ROLE_CUSTOMER).ToList();
            }

            return users;
        }

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

        [HttpPut]
        public async Task<ActionResult> Update(ApplicationUser user)
        {
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
