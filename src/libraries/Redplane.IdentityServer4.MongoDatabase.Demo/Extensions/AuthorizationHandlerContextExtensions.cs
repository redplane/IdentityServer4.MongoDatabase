using System.Linq;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Redplane.IdentityServer4.MongoDatabase.Demo.Attributes;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Extensions
{
    public static class AuthorizationHandlerContextExtensions
    {
        #region Methods

        public static bool IsAllowAnonymous(this AuthorizationFilterContext context)
        {
            if (context.Filters == null)
                return false;

            return context.Filters.Any(x => x is LenientAuthorizationAttribute);
        }

        public static void MarkRequirementAsFailed(this AuthorizationHandlerContext context,
            IAuthorizationRequirement requirement)
        {
            var authorizationFilterContext = (AuthorizationFilterContext) context.Resource;

            if (authorizationFilterContext.IsAllowAnonymous())
            {
                if (context.User.Identities.All(x => x.Name != "Anonymous"))
                    context.User.AddIdentity(new GenericIdentity("Anonymous"));

                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }

        #endregion
    }
}