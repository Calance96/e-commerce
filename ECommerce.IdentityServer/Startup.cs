// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ECommerce.DataAccess;
using Microsoft.EntityFrameworkCore;
using ECommerce.Models;
using System;
using Microsoft.AspNetCore.Http;
using ECommerce.IdentityServer.Controllers;
using IdentityServer4.Services;

namespace ECommerce.IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(configs =>
            {
                configs.Password.RequiredLength = 6;
                configs.Password.RequireDigit = false;
                configs.Password.RequireNonAlphanumeric = false;
                configs.Password.RequireUppercase = false;
                configs.Password.RequireLowercase = false;
            })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Authentication.CookieLifetime = TimeSpan.FromHours(8);
                options.Authentication.CookieSlidingExpiration = true;
                options.Authentication.CookieSameSiteMode = SameSiteMode.None;

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ProfileService>()
                .AddDeveloperSigningCredential();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}