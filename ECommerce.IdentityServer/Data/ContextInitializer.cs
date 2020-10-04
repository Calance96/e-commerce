using ECommerce.DataAccess;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.IdentityServer.Data
{
    public class ContextInitializer
    {
        private readonly ConfigurationDbContext _context;

        public ContextInitializer(ConfigurationDbContext context)
        {
            _context = context;
        }

        public void SeedData()
        {
            if (_context.Database.GetPendingMigrations().Count() > 0)
            {
                _context.Database.Migrate();
            }

            if (!_context.IdentityResources.Any())
            {
                foreach (var identityResource in Config.IdentityResources.ToList())
                {
                    _context.IdentityResources.Add(identityResource.ToEntity());
                }
                _context.SaveChanges();
            }

            if (!_context.ApiScopes.Any())
            {
                foreach (var apiScope in Config.ApiScopes.ToList())
                {
                    _context.ApiScopes.Add(apiScope.ToEntity());
                }
                _context.SaveChanges();
            }

            if (!_context.ApiResources.Any())
            {
                foreach (var apiResource in Config.ApiResources.ToList())
                {
                    _context.ApiResources.Add(apiResource.ToEntity());
                }
                _context.SaveChanges();
            }

            if (!_context.Clients.Any())
            {
                foreach (var client in Config.Clients.ToList())
                {
                    _context.Clients.Add(client.ToEntity());
                }
                _context.SaveChanges();
            }
        }
    }
}
