using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Redplane.IdentityServer4.MongoDatabase.Constants;
using Redplane.IdentityServer4.MongoDatabase.Demo.Extensions;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Interfaces;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        #region Constructor

        public ResourceOwnerPasswordValidator(
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IOptions<IdentityServerSettings> identityServerSettingsOptions)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _userService = userService;

            _identityServerSettings = identityServerSettingsOptions.Value;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // Hash the password.
            var username = context.UserName;
            var password = context.Password;
            
            var shouldPasswordIgnored = false;

#if DEBUG
            shouldPasswordIgnored = _identityServerSettings.IgnorePasswordValidation;
#endif
            // Find the user in the system with defined username and password.
            var user = await _userService.BasicLoginAsync(username, password, shouldPasswordIgnored);

            // No user is found.
            if (user == null)
            {
                context.Result =
                    new GrantValidationResult(TokenRequestErrors.InvalidGrant,
                        HttpMessageCodeConstants.InvalidUsernameOrPassword);
                return;
            }

            var userCredential = new UserCredential(user);
            context.Result = new GrantValidationResult(
                user.Id.ToString("D"), GrantType.ResourceOwnerPassword,
                userCredential.ToClaims());

            _httpContext.SetProfile(userCredential);
        }

        #endregion

        #region Properties

        private readonly IUserService _userService;

        private readonly HttpContext _httpContext;

        private readonly IdentityServerSettings _identityServerSettings;

        #endregion
    }
}