using System;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.MongoDbAdapter.Constants;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IdentityServer4.MongoDbAdapter.Demo.Services.Implementations
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        #region Constructor

        public ResourceOwnerPasswordValidator(
            IHttpContextAccessor httpContextAccessor,
            IOptions<IdentityServerSettings> identityServerSettingsOptions)
        {
            _httpContext = httpContextAccessor.HttpContext;

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

            try
            {
                var shouldPasswordIgnored = false;

#if DEBUG
                shouldPasswordIgnored = _identityServerSettings.IgnorePasswordValidation;
#endif
                // Find the user in the system with defined username and password.
                //var user = await _userFactory.BasicLoginAsync(username, password, shouldPasswordIgnored);

                //// No user is found.
                //if (user == null)
                //{
                //    context.Result =
                //        new GrantValidationResult(TokenRequestErrors.InvalidGrant,
                //            HttpMessageCodeConstants.InvalidUsernameOrPassword);
                //    return;
                //}

                //var userCredential = new UserCredential(user);
                //context.Result = new GrantValidationResult(
                //    user.Id.ToString("D"), GrantType.ResourceOwnerPassword,
                //    userCredential.ToClaims());

                //_httpContext.SetProfile(user);
            }
            catch (Exception exception)
            {
                //if (exception is HttpResponseException httpResponseException)
                //    if (httpResponseException.StatusCode == HttpStatusCode.NotFound)
                //    {
                //        context.Result =
                //            new GrantValidationResult(TokenRequestErrors.InvalidGrant,
                //                HttpMessageCodeConstants.InvalidUsernameOrPassword);

                //        return;
                //    }

                throw;
            }
        }

        #endregion

        #region Properties

        //private readonly IUserFactory _userFactory;

        private readonly HttpContext _httpContext;

        private readonly IdentityServerSettings _identityServerSettings;

        #endregion
    }
}