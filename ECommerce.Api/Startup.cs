using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using RestSharp.Serializers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ECommerce.Api
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
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
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

            services.AddControllers();
            services.AddScoped<IDbInitializer, DbInitializer>();
            
            services.AddSwaggerGen(swaggerGenOptions =>
            {
                swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo { 
                    Version = "v1", 
                    Title = "E-Mall API", 
                    Description = "A simple Web API build based on ASP.NET Core 3.1"
                });
                swaggerGenOptions.CustomOperationIds(e => e.ActionDescriptor.RouteValues["action"]);
                swaggerGenOptions.DocumentFilter<GenerateJsonFilter>();
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                swaggerGenOptions.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Mall API v1");
                setupAction.RoutePrefix = string.Empty;
            });

            app.UseSerilogRequestLogging(); // Serilog middleware to know what requests the app is handling

            app.UseRouting();

            app.UseAuthorization();
            dbInitializer.Initialize();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    
    public class GenerateJsonFilter : IDocumentFilter
    {
        private readonly IWebHostEnvironment _environment;

        public GenerateJsonFilter(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            GenerateJson(swaggerDoc);
        }

        private void GenerateJson(OpenApiDocument swaggerDoc)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Swagger.json");
            var jsonContent = swaggerDoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            if (!File.Exists(filePath) ||
                string.Compare(File.ReadAllText(filePath), jsonContent, StringComparison.OrdinalIgnoreCase) != 0)
            {
                File.WriteAllText(filePath, jsonContent);
            }
        }
    }
}
