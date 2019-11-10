using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.MongoDbAdapter.Demo.Extensions;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using IdentityServer4.MongoDbAdapter.Demo.Services.Interfaces;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.MongoDbAdapter.Demo.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        #region Constructor

        public ProfileService(
            ILogger<ProfileService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;
        }

        #endregion

        #region Properties

        private readonly HttpContext _httpContext;

        private readonly ILogger<ProfileService> _logger;

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // Get user which is attached to http context.
            var userCredential = _httpContext.FindProfile(true);

            context.IssuedClaims = userCredential.ToClaims()?.ToList();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task IsActiveAsync(IsActiveContext context)
        {
            // Get subject from context (set in ResourceOwnerPasswordValidator.ValidateAsync),
            var subject = context.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject)?.Value;
            if (string.IsNullOrEmpty(subject) || !Guid.TryParse(subject, out var userId))
            {
                context.IsActive = false;
                return Task.CompletedTask;
            }

            try
            {
                var userCredential = new UserCredential(context.Subject.Claims.ToList());

                // Update http context profile.
                _httpContext.SetProfile(userCredential);

                context.IsActive = true;
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                return Task.CompletedTask;
            }
        }

        #endregion
    }
}