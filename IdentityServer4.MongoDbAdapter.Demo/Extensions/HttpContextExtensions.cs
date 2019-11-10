using System;
using System.Net;
using System.Security.Claims;
using IdentityServer4.MongoDbAdapter.Constants;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.MongoDbAdapter.Demo.Extensions
{
    public static class HttpContextExtensions
    {
        #region Methods

        /// <summary>
        ///     Find user profile that attached to HttpContext.
        /// </summary>
        /// <returns></returns>
        public static UserCredential FindProfile(this HttpContext httpContext, bool throwException = false)
        {
            if (httpContext == null)
            {
                if (throwException)
                    throw new UnauthorizedAccessException(HttpMessageCodeConstants.InvalidUsernameOrPassword);

                return null;
            }

            var items = httpContext.Items;

            var info = items?[ClaimTypes.UserData];
            if (info == null || !(info is UserCredential userCredential))
            {
                if (throwException)
                    throw new UnauthorizedAccessException(HttpMessageCodeConstants.InvalidUsernameOrPassword);

                return null;
            }


            return userCredential;
        }

        /// <summary>
        ///     Set profile to HttpContext.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="userCredential"></param>
        public static void SetProfile(this HttpContext httpContext, UserCredential userCredential)
        {
            httpContext.Items[ClaimTypes.UserData] = userCredential;
        }

        #endregion
    }
}