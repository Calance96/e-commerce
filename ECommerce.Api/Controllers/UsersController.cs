using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{role}")]
        public async Task<IEnumerable<ApplicationUser>> GetAll(string role)
        {
            var users = await _context.ApplicationUsers.ToListAsync();
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
                return NotFound();
            }

            return user;
        }

        [HttpPut]
        public async Task Update(ApplicationUser user)
        {
            _context.ApplicationUsers.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
