using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityServer4.MongoDbAdapter.Demo.Attributes
{
    public class LenientAuthorizationAttribute : Attribute, IFilterMetadata
    {
    }
}