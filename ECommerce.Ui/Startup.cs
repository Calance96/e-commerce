using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;

namespace ECommerce.Ui
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AddApiAcessServices(services);
            services.AddHttpContextAccessor();

            services.AddSession(configs =>
            {
                configs.IdleTimeout = TimeSpan.FromMinutes(30);
                configs.Cookie.HttpOnly = true; // Mitigate the risk of client side script accessing the protected cookie 
                configs.Cookie.IsEssential = true;
            });

            services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, configs =>
            {
                configs.Cookie.Name = "EMall";
                configs.LoginPath = "/Account/Login";
                configs.LogoutPath = "/Account/Logout";
                configs.AccessDeniedPath = "/AccessDenied";
                configs.SlidingExpiration = true;
                configs.ExpireTimeSpan = TimeSpan.FromHours(24);
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(SD.Policy.ADMIN_ONLY, policy => policy.RequireRole(SD.ROLE_ADMIN));
                options.AddPolicy(SD.Policy.CUSTOMER_ONLY, policy => policy.RequireRole(SD.ROLE_CUSTOMER));
                options.AddPolicy(SD.Policy.AUTHENTICATED_ONLY, policy => policy.RequireAuthenticatedUser());
            });

            services.AddRazorPages()
                .AddRazorRuntimeCompilation()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeAreaFolder("Admin", "/Category", SD.Policy.ADMIN_ONLY);
                    options.Conventions.AuthorizeAreaFolder("Admin", "/Management", SD.Policy.ADMIN_ONLY);
                    options.Conventions.AuthorizeAreaFolder("Admin", "/Order", SD.Policy.ADMIN_ONLY);
                    options.Conventions.AuthorizeAreaFolder("Admin", "/Product", SD.Policy.ADMIN_ONLY);

                    options.Conventions.AuthorizeAreaPage("Customer", "/Order", SD.Policy.CUSTOMER_ONLY);
                    options.Conventions.AuthorizeAreaPage("Customer", "/ShoppingCart", SD.Policy.CUSTOMER_ONLY);
                    options.Conventions.AuthorizeAreaPage("Item", "/Details", SD.Policy.CUSTOMER_ONLY);

                    options.Conventions.AuthorizeAreaFolder("Account", "/Profile", SD.Policy.AUTHENTICATED_ONLY);
                })
                .AddSessionStateTempDataProvider();

            services.Configure<StripeConfigs>(Configuration.GetSection("StripeConfigs"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            StripeConfiguration.ApiKey = Configuration["StripeConfigs:SecretKey"];

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        private void AddApiAcessServices(IServiceCollection services)
        {
            services.AddHttpClient<CategoryService>(configs =>
            {
                configs.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                configs.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient<Services.ProductService>(configs =>
            {
                configs.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                configs.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient<CartService>(configs =>
            {
                configs.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                configs.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient<Services.OrderService>(configs =>
            {
                configs.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                configs.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient<UserService>(configs =>
            {
                configs.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                configs.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient<AuthService>(configs =>
            {
                configs.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                configs.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }
    }
}
