using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            services.AddDbContext<ApplicationDbContext>(configs => {
                configs.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

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

            services.ConfigureApplicationCookie(configs =>
            {
                configs.Cookie.Name = "EMall.Cookie";
                configs.LoginPath = "/Account/Login";
                configs.LogoutPath = "/Account/Logout";
                configs.AccessDeniedPath = "/AccessDenied";
                configs.SlidingExpiration = true;
                configs.ExpireTimeSpan = TimeSpan.FromHours(24);
            });

            AddApiAcessServices(services);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppConstant.ROLE_ADMIN));
                options.AddPolicy("CustomerOnly", policy => policy.RequireRole(AppConstant.ROLE_CUSTOMER));
            });

            services.AddRazorPages()
                .AddRazorRuntimeCompilation()
                .AddRazorPagesOptions(options => 
                {
                    options.Conventions.AuthorizeAreaFolder("Admin", "/Category", "AdminOnly");
                    options.Conventions.AuthorizeAreaFolder("Admin", "/Product", "AdminOnly");
                    options.Conventions.AuthorizeAreaPage("Item", "/Details", "CustomerOnly");
                    options.Conventions.AuthorizeAreaPage("Customer", "/ShoppingCart", "CustomerOnly");
                });

            services.AddSession(configs =>
            {
                configs.IdleTimeout = TimeSpan.FromMinutes(30);
                configs.Cookie.HttpOnly = true; // Mitigate the risk of client side script accessing the protected cookie 
                configs.Cookie.IsEssential = true;
            });

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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            StripeConfiguration.ApiKey = Configuration["StripeConfigs:SecretKey"];
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

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
        }
    }
}
