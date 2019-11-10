using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.MongoDbAdapter.Demo.AuthorizationRequirements;
using IdentityServer4.MongoDbAdapter.Demo.Enums;
using IdentityServer4.MongoDbAdapter.Demo.Extensions;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using IdentityServer4.MongoDbAdapter.Demo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityServer4.MongoDbAdapter.Demo.AuthorizationHandlers
{
    public class SolidUserRequirementHandler : AuthorizationHandler<SolidUserRequirement>
    {
        #region Constructor

        public SolidUserRequirementHandler(
            IHttpContextAccessor httpContextAccessor,
            IUserService userService)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            SolidUserRequirement requirement)
        {
            // Convert authorization filter context into authorization filter context.
            var authorizationFilterContext = (AuthorizationFilterContext) context.Resource;

            // Get http context.
            var httpContext = authorizationFilterContext.HttpContext;

            //// Decode access token.
            var claims = httpContext.User.Claims.ToList();
            if (!claims.Any())
            {
                context.MarkRequirementAsFailed(requirement);
                return Task.CompletedTask;
            }

            // Id is not in claims.
            var idClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (idClaim == null || !Guid.TryParse(idClaim.Value, out var id))
            {
                context.MarkRequirementAsFailed(requirement);
                return Task.CompletedTask;
            }

            _httpContext.SetProfile(new UserCredential(claims));
            context.Succeed(requirement);

            return Task.CompletedTask;
        }

        #endregion

        #region Properties

        private readonly HttpContext _httpContext;

        #endregion
    }
}