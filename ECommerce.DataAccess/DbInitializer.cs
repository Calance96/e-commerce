using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommerce.DataAccess
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(ApplicationDbContext context, 
                                UserManager<ApplicationUser> userManager, 
                                RoleManager<IdentityRole> roleManager,
                                ILogger<DbInitializer> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }


                if (!_context.Roles.Any(r => r.Name == SD.ROLE_ADMIN))
                {
                    AddRolesAndCreateAdminUser();
                }

                if (!_context.EntityActionTypes.Any(o => o.ActionName == "Update"))
                {
                    AddEntityActionTypes();
                }

                if (!_context.OrderActions.Any(o => o.ActionName == "Process"))
                {
                    AddOrderActions();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing database initializer.");
            }
        }

        private void AddOrderActions()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT [dbo].[OrderActions] ON");
                _context.OrderActions.Add(new OrderAction
                {
                    Id = (long)SD.OrderAction.PROCESS,
                    ActionName = "Process"
                });
                _context.OrderActions.Add(new OrderAction
                {
                    Id = (long)SD.OrderAction.SHIP,
                    ActionName = "Ship"
                });
                _context.OrderActions.Add(new OrderAction
                {
                    Id = (long)SD.OrderAction.CANCEL,
                    ActionName = "Cancel"
                });
                _context.OrderActions.Add(new OrderAction
                {
                    Id = (long)SD.OrderAction.REFUND,
                    ActionName = "Refund"
                });
                _context.OrderActions.Add(new OrderAction
                {
                    Id = (long)SD.OrderAction.COMPLETE,
                    ActionName = "Complete"
                });
                _context.SaveChanges();
                _context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT [dbo].[OrderActions] OFF");
                transaction.Commit();
            }
        }

        private void AddEntityActionTypes()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT [dbo].[EntityActionTypes] ON");
                _context.EntityActionTypes.Add(new EntityActionType
                {
                    Id = (long)SD.EntityActionType.Update,
                    ActionName = "Update"
                });
                _context.EntityActionTypes.Add(new EntityActionType
                {
                    Id = (long)SD.EntityActionType.Delete,
                    ActionName = "Delete"
                });
                _context.SaveChanges();
                _context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT [dbo].[EntityActionTypes] OFF");
                transaction.Commit();
            }
        }

        private void AddRolesAndCreateAdminUser()
        {
            _roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.ROLE_CUSTOMER)).GetAwaiter().GetResult();

            ApplicationUser adminUser = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                Name = "Admin User",
                PhoneNumber = "0123456789",
                CreatedAt = DateTime.Now
            };

            _userManager.CreateAsync(adminUser, "admin123").GetAwaiter().GetResult();
            adminUser = _userManager.FindByEmailAsync(adminUser.Email).GetAwaiter().GetResult();

            adminUser.CreatedBy = adminUser.Id;
            _userManager.UpdateAsync(adminUser).GetAwaiter().GetResult();

            _userManager.AddToRoleAsync(adminUser, SD.ROLE_ADMIN).GetAwaiter().GetResult();
        }
    }
}
