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
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly PersistedGrantDbContext _persistedGrantDbContext;

        public ContextInitializer(ConfigurationDbContext configurationDbContext,
                                  PersistedGrantDbContext persistedGrantDbContext)
        {
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
        }

        public void SeedData()
        {
            if (_configurationDbContext.Database.GetPendingMigrations().Count() > 0)
            {
                _configurationDbContext.Database.Migrate();
            }

            if (_persistedGrantDbContext.Database.GetPendingMigrations().Count() > 0)
            {
                _persistedGrantDbContext.Database.Migrate();
            }

            if (!_configurationDbContext.IdentityResources.Any())
            {
                foreach (var identityResource in Config.IdentityResources.ToList())
                {
                    _configurationDbContext.IdentityResources.Add(identityResource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiScopes.Any())
            {
                foreach (var apiScope in Config.ApiScopes.ToList())
                {
                    _configurationDbContext.ApiScopes.Add(apiScope.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                foreach (var apiResource in Config.ApiResources.ToList())
                {
                    _configurationDbContext.ApiResources.Add(apiResource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.Clients.Any())
            {
                foreach (var client in Config.Clients.ToList())
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }
        }
    }
}
