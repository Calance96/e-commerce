using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace ECommerce.Api.SwaggerConfigs
{
    public class SwaggerGenConfiguration : IConfigureOptions<SwaggerGenOptions>
    {
        public IApiVersionDescriptionProvider _apiVersionProvider { get; }

        public SwaggerGenConfiguration(IApiVersionDescriptionProvider apiVersionProvider)
        {
            _apiVersionProvider = apiVersionProvider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach(var desc in _apiVersionProvider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(name: desc.GroupName, new OpenApiInfo
                {
                    Version = desc.GroupName,
                    Title = $"E-Mall API {desc.GroupName}"
                });
            }

            options.DocumentFilter<GenerateJsonFilter>();
            options.OperationFilter<ApiVersionOperationFilter>();
            options.OperationFilter<AuthorizationOperationFilter>();
            options.CustomOperationIds(apiDescription => apiDescription.ActionDescriptor.RouteValues["action"]);

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            // Add the authorize button at top right of swagger document
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "JWT access token using the bearer scheme",
                Type = SecuritySchemeType.ApiKey
            });
        }
    }
}
