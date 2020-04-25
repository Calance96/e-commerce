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

            if (role == AppConstant.ROLE_ADMIN)
            {
                users = users.Where(user => user.Role == AppConstant.ROLE_ADMIN).ToList();
            } 
            else if (role == AppConstant.ROLE_CUSTOMER)
            {
                users = users.Where(user => user.Role == AppConstant.ROLE_CUSTOMER).ToList();
            }

            return users;
        }
    }
}
