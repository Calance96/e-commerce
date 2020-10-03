using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace ECommerce.Api.Authorization
{
    public class JwtTokenAuthorizationHandler : AuthorizationHandler<JwtTokenAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, JwtTokenAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
