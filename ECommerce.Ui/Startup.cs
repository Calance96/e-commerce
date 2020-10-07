using System;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Threading.Tasks;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
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
            var isIdentityServerEnabled = Convert.ToBoolean(Configuration["IdentityProvider:IsEnabled"]);

            services.AddHttpContextAccessor();
            services.AddSession(configs =>
            {
                configs.IdleTimeout = TimeSpan.FromMinutes(30);
                configs.Cookie.HttpOnly = true; // Mitigate the risk of client side script accessing the protected cookie 
                configs.Cookie.IsEssential = true;
                configs.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = isIdentityServerEnabled ? OpenIdConnectDefaults.AuthenticationScheme : CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.AccessDeniedPath = "/AccessDenied";
                //options.EventsType = typeof(CustomCookieAuthenticationEvents);
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                if (!isIdentityServerEnabled)
                {
                    options.Cookie.Name = "EMall";
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                }
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = Configuration["IdentityProvider:Authority"];
                options.RequireHttpsMetadata = true;

                options.ClientId = "ecommerce-client";
                options.ClientSecret = "secret";

                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.CallbackPath = "/signin-oidc";
                options.SignedOutRedirectUri = "/index";

                options.Scope.Add("main_api");
                options.Scope.Add("offline_access");

                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;

                options.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.Role, JwtClaimTypes.Role);
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role
                };

                options.Events = new OpenIdConnectEvents
                {
                    OnAccessDenied = (ctx) =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.Redirect("/", true);
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(SD.Policy.ADMIN_ONLY, policy => policy.RequireAuthenticatedUser().RequireRole(SD.ROLE_ADMIN));
                options.AddPolicy(SD.Policy.CUSTOMER_ONLY, policy => policy.RequireAuthenticatedUser().RequireRole(SD.ROLE_CUSTOMER));
                options.AddPolicy(SD.Policy.AUTHENTICATED_ONLY, policy => policy.RequireAuthenticatedUser());
            });
            //services.AddScoped<CustomCookieAuthenticationEvents>();

            services.AddHttpClient("api", async (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var token = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");

                if (token != null)
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }
            });

            services.AddScoped<AuthService>();
            services.AddScoped<CartService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<Services.OrderService>();
            services.AddScoped<Services.ProductService>();
            services.AddScoped<UserService>();


            services.AddRazorPages()
                .AddRazorRuntimeCompilation()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeAreaFolder("Admin", "/", SD.Policy.ADMIN_ONLY);
                    options.Conventions.AuthorizeAreaFolder("Customer", "/", SD.Policy.CUSTOMER_ONLY);
                    options.Conventions.AuthorizeAreaFolder("Item", "/", SD.Policy.AUTHENTICATED_ONLY);
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
    }
}
