using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Api.SwaggerConfigs
{
    public class ApiVersionProcessor : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                return;

            var apiVersionParam = operation.Parameters.FirstOrDefault(p => p.Name == "api-version");
            var apiVersionDescription = context.ApiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == "api-version");

            if (apiVersionParam.Schema.Default == null && apiVersionDescription.DefaultValue != null)
            {
                apiVersionParam.Schema.Default = new OpenApiString(apiVersionDescription.DefaultValue.ToString()); 
            }
        }
    }
}
